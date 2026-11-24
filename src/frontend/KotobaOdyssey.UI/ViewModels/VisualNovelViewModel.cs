using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KotobaOdyssey.Core.DTOs;
using KotobaOdyssey.Core.Enums;
using KotobaOdyssey.Core.Interfaces;
using KotobaOdyssey.Core.Models;

namespace KotobaOdyssey.UI.ViewModels;

public partial class VisualNovelViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ICharacterDialogueService? _characterService;
    private readonly IVocabularyRepository? _vocabularyRepository;
    private readonly IUserProgressRepository? _progressRepository;

    [ObservableProperty]
    private CharacterProfileDto? _selectedCharacter;

    [ObservableProperty]
    private string _currentSpeakerName = "ジョバンニ (Giovanni)";

    [ObservableProperty]
    private string _currentSpeechJapanese = "「君もこの銀河鉄道に乗ってきたんだね！ カムパネルラと一緒に、夜空の旅をしているところだよ。何でも話してね。」";

    [ObservableProperty]
    private string _currentSpeechVietnamese = "\"Cậu cũng bước lên chuyến tàu Ngân Hà này sao! Mình đang cùng Campanella du hành giữa bầu trời đêm đấy. Cứ thoải mái trò chuyện với mình nhé.\"";

    [ObservableProperty]
    private string _currentExpression = "✨ Tươi cười";

    [ObservableProperty]
    private string _grammarFeedback = "Bạn mở đầu cuộc trò chuyện rất thân thiện! Hãy tiếp tục đặt câu hỏi bằng tiếng Nhật nhé.";

    [ObservableProperty]
    private string _userMessageInput = string.Empty;

    [ObservableProperty]
    private string _currentMode = "free"; // "free", "practice", "scenario"

    [ObservableProperty]
    private bool _hasCorrection;

    [ObservableProperty]
    private string _correctedSentence = string.Empty;

    [ObservableProperty]
    private string _correctionExplanation = string.Empty;

    [ObservableProperty]
    private string _activeBackgroundPath = "pack://application:,,,/KotobaOdyssey.UI;component/Assets/Images/hero_astral.jpg";

    public ObservableCollection<CharacterProfileDto> Characters { get; } = new();
    public ObservableCollection<CharacterVocabItemDto> CurrentKeyVocabulary { get; } = new();
    public ObservableCollection<string> SuggestedFollowups { get; } = new();

    public VisualNovelViewModel(
        INavigationService navigationService,
        ICharacterDialogueService? characterService = null,
        IVocabularyRepository? vocabularyRepository = null,
        IUserProgressRepository? progressRepository = null)
    {
        _navigationService = navigationService;
        _characterService = characterService;
        _vocabularyRepository = vocabularyRepository;
        _progressRepository = progressRepository;

        _ = InitializeCharactersAsync();
    }

    public async Task InitializeCharactersAsync()
    {
        if (_characterService == null) return;

        try
        {
            var profiles = await _characterService.GetProfilesAsync();
            Characters.Clear();
            foreach (var p in profiles)
            {
                Characters.Add(p);
            }

            if (Characters.Count > 0)
            {
                SelectedCharacter = Characters[0];
            }
        }
        catch { }
    }

    partial void OnSelectedCharacterChanged(CharacterProfileDto? value)
    {
        if (value == null) return;
        CurrentSpeakerName = $"{value.JapaneseName} ({value.Name})";
        CurrentSpeechJapanese = $"「こんにちは！ {value.PronounFirstPerson}と一緒に物語の世界を旅しよう。日本語で何でも話しかけてね。」";
        CurrentSpeechVietnamese = $"\"Xin chào! Hãy cùng {value.Name} du hành vào thế giới của những câu chuyện nhé. Cứ thoải mái trò chuyện với mình bằng tiếng Nhật.\"";
        CurrentExpression = "✨ Dịu dàng";
        GrammarFeedback = $"Nhân vật {value.Name}: {value.Personality}";
        HasCorrection = false;

        CurrentKeyVocabulary.Clear();
        SuggestedFollowups.Clear();
        foreach (var prompt in value.SuggestedPrompts)
        {
            SuggestedFollowups.Add(prompt);
        }
    }

    [RelayCommand]
    public void SetMode(string mode)
    {
        CurrentMode = mode;
        if (mode == "practice")
        {
            GrammarFeedback = "Chế độ Luyện giao tiếp: Nhân vật sẽ hỗ trợ và hướng dẫn cách diễn đạt tiếng Nhật tự nhiên.";
        }
        else if (mode == "scenario")
        {
            GrammarFeedback = "Chế độ Nhập vai: Tham gia tình huống phân cảnh trạm dừng chân Ngân Hà.";
        }
        else
        {
            GrammarFeedback = "Chế độ Tự do: Trò chuyện thoải mái theo tính cách nhân vật.";
        }
    }

    [RelayCommand]
    public async Task SendMessageAsync(string? quickPrompt = null)
    {
        string message = (quickPrompt ?? UserMessageInput).Trim();
        if (string.IsNullOrWhiteSpace(message) || _characterService == null || SelectedCharacter == null) return;

        UserMessageInput = string.Empty;
        IsBusy = true;
        StatusText = "Nhân vật đang suy nghĩ câu trả lời...";

        try
        {
            var response = await _characterService.ChatAsync(new CharacterChatRequest
            {
                CharacterId = SelectedCharacter.CharacterId,
                NovelId = SelectedCharacter.NovelId ?? 1,
                ChapterNumber = 1,
                UserMessage = message,
                JlptLevel = "N3",
                ConversationMode = CurrentMode
            });

            if (response.Success)
            {
                CurrentSpeechJapanese = response.ReplyJapanese;
                CurrentSpeechVietnamese = response.ReplyVietnamese;
                GrammarFeedback = response.GrammarFeedback;

                CurrentExpression = response.Expression switch
                {
                    "happy" => "✨ Tươi cười",
                    "gentle" => "🌸 Dịu dàng",
                    "thinking" => "🤔 Trầm tư",
                    "surprised" => "😲 Ngạc nhiên",
                    "sad" => "💧 Thoáng buồn",
                    _ => "✦ Bình thản"
                };

                // Handle Gentle Correction
                if (response.Correction != null && response.Correction.HasCorrection)
                {
                    HasCorrection = true;
                    CorrectedSentence = response.Correction.CorrectedSentence;
                    CorrectionExplanation = response.Correction.Explanation;
                }
                else
                {
                    HasCorrection = false;
                }

                // Handle Vocab
                CurrentKeyVocabulary.Clear();
                foreach (var v in response.KeyVocabulary)
                {
                    CurrentKeyVocabulary.Add(v);
                }

                // Handle Followups
                SuggestedFollowups.Clear();
                foreach (var f in response.SuggestedFollowups)
                {
                    SuggestedFollowups.Add(f);
                }

                // Award XP
                if (_progressRepository != null)
                {
                    await _progressRepository.AddXpAsync(5);
                }
            }
        }
        catch (Exception ex)
        {
            GrammarFeedback = $"Lỗi kết nối: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveVocabToSrsAsync(CharacterVocabItemDto item)
    {
        if (_vocabularyRepository == null || item == null) return;

        var vocabItem = new VocabularyItem
        {
            Word = item.Word,
            Reading = item.Reading,
            Meaning = item.Meaning,
            JlptLevel = Enum.TryParse<JlptLevel>(item.JlptLevel, out var jlpt) ? jlpt : JlptLevel.N3,
            ContextSentence = CurrentSpeechJapanese,
            SourceNovelTitle = SelectedCharacter?.NovelTitle ?? "Visual Novel",
            EaseFactor = 2.5
        };

        await _vocabularyRepository.AddAsync(vocabItem);
        StatusText = $"Đã lưu từ '{item.Word}' vào Flashcard SRS!";
    }

    [RelayCommand]
    public void BackToLibrary()
    {
        _navigationService.NavigateTo("Library");
    }
}

