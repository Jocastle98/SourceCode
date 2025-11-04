#include "MyWidget.h"
#include "HttpPostActor.h"
#include "Materials/MaterialInstanceConstant.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "Components/HorizontalBox.h"
#include "Components/VerticalBox.h"
#include "Components/Image.h"
#include "Components/VerticalBoxSlot.h"
#include "Components/OverlaySlot.h"
#include "MyGameInstance.h"
#include "HAL/PlatformFilemanager.h"
#include "GenericPlatform/GenericPlatformFile.h"
#include "Misc/FileHelper.h"
#include "DesktopPlatform/Public/IDesktopPlatform.h"
#include "DesktopPlatform/Public/DesktopPlatformModule.h"
#include "JsonUtilities/Public/JsonUtilities.h"
#include "Engine/Font.h"
#include "TimerManager.h"
#include "UObject/ObjectMacros.h"
#include "CoreMinimal.h"

#include "JsonObjectConverter.h"
#include "Json.h"


UMyWidget::UMyWidget(const FObjectInitializer& ObjectInitializer)
    : Super(ObjectInitializer)
{
    TestChatListInstance = ObjectInitializer.CreateDefaultSubobject<UTestChatList>(this, TEXT("TestChatListInstance"));
    CurrentLogIndex = 0;
}



void UMyWidget::NativeConstruct()
{
    Super::NativeConstruct();

    if (bowButton)
    {
        bowButton->OnClicked.AddDynamic(this, &UMyWidget::OnBowButtonClicked);
    }
    if (shakeButton)
    {
        shakeButton->OnClicked.AddDynamic(this, &UMyWidget::OnShakeButtonClicked);
    }
    if (sorryButton)
    {
        sorryButton->OnClicked.AddDynamic(this, &UMyWidget::OnSorryButtonClicked);
    }
    if (worryButton)
    {
        worryButton->OnClicked.AddDynamic(this, &UMyWidget::OnWorryButtonClicked);
    }
    if (shockButton)
    {
        shockButton->OnClicked.AddDynamic(this, &UMyWidget::OnShockButtonClicked);
    }
    if (patButton)
    {
        patButton->OnClicked.AddDynamic(this, &UMyWidget::OnPatButtonClicked);
    }
    if (agreeingButton)
    {
        agreeingButton->OnClicked.AddDynamic(this, &UMyWidget::OnAgreeingButtonClicked);
    }
    if (IsValid(bowButton))
    {
        // 클릭 이벤트를 코드로 등록합니다.
        bowButton->OnClicked.AddDynamic(this, &UMyWidget::OnBowButtonClicked);
    }

    if (SaveBtn)
    {
        SaveBtn->OnClicked.AddDynamic(this, &UMyWidget::OnSaveBtnClicked);
    }
    if (RecallBtn)
    {
        RecallBtn->OnClicked.AddDynamic(this, &UMyWidget::OnRecallBtnClicked);
    }
    if (RenderBtn)
    {
        RenderBtn->OnClicked.AddDynamic(this, &UMyWidget::OnRenderBtnClicked);
    }

    ResultText = Cast<UTextBlock>(GetWidgetFromName(TEXT("ResultText")));

    if (ResultText)
    {
        ResultText->SetText(FText::FromString(TEXT("")));
    }

    if (SadButton == nullptr)
    {
        UE_LOG(LogTemp, Error, TEXT("SadButton is not valid"));
        return;
    }

    for (TObjectIterator<AHttpPostActor> It; It; ++It)
    {
        if (It->GetWorld() == GetWorld())
        {
            HttpPost = *It;
            break;
        }
    }

    if (HttpPost != nullptr)
    {
        UE_LOG(LogTemp, Warning, TEXT("HttpPost is initialized in NativeConstruct."));
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("No AHttpPostActor was found in the level!"));
    }

    if (!TestChatListInstance)
    {
        TestChatListInstance = NewObject<UTestChatList>(this);
        UE_LOG(LogTemp, Warning, TEXT("TestChatListInstance is valid!"));
        // If it's still null, there's a bigger problem.
        if (!TestChatListInstance)
        {
            UE_LOG(LogTemp, Error, TEXT("Failed to create TestChatListInstance in UMyWidget::NativeConstruct"));
            return;
        }
    }

    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance)
    {
        // 콤보 박스를 비웁니다.
        //namecombo->ClearOptions();

        // Slot1CharacterName과 Slot2CharacterName이 비어있지 않다면 콤보 박스에 추가합니다.
        if (!GameInstance->Slot1CharacterName.IsEmpty())
        {
            namecombo->AddOption(GameInstance->Slot1CharacterName);
        }

        if (!GameInstance->Slot2CharacterName.IsEmpty())
        {
            namecombo->AddOption(GameInstance->Slot2CharacterName);
        }

        // 필요한 경우, 콤보 박스의 선택을 업데이트합니다.
        if (namecombo->GetOptionCount() > 0)
        {
            namecombo->SetSelectedOption(namecombo->GetOptionAtIndex(0)); // 첫 번째 옵션을 선택합니다.
        }
    }



}

void UMyWidget::DeleteFilesInFolder(const FString& FolderPath)
{
    IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

    // 폴더가 실제로 존재하는지 확인합니다.
    if (PlatformFile.DirectoryExists(*FolderPath))
    {
        // 폴더 내의 모든 wav 파일 경로를 찾습니다.
        TArray<FString> FilesToDelete;
        PlatformFile.FindFiles(FilesToDelete, *FolderPath, TEXT("*.wav"));

        // 각 파일에 대해 삭제를 시도합니다.
        for (const FString& File : FilesToDelete)
        {
            FString FullPath = FPaths::Combine(FolderPath, File);
            if (PlatformFile.DeleteFile(*FullPath))
            {
                // 파일이 성공적으로 삭제됨
                UE_LOG(LogTemp, Warning, TEXT("Successfully deleted file: %s"), *FullPath);
            }
            else
            {
                // 파일 삭제 실패. 오류 처리
                UE_LOG(LogTemp, Warning, TEXT("Failed to delete file: %s"), *FullPath);
            }
        }
    }
    else
    {
        // 폴더가 존재하지 않음
        UE_LOG(LogTemp, Warning, TEXT("Directory does not exist: %s"), *FolderPath);
    }
}

void UMyWidget::ResetButtonColors()
{
    if (SadButton)
    {
        SadButton->BackgroundColor = FLinearColor::White; //                     
        SadButton->SynchronizeProperties();
    }
    if (JoyButton)
    {
        JoyButton->BackgroundColor = FLinearColor::White; //                     
        JoyButton->SynchronizeProperties();
    }
    if (FearButton)
    {
        FearButton->BackgroundColor = FLinearColor::White; //                     
        FearButton->SynchronizeProperties();
    }

    if (AngryButton)
    {
        AngryButton->BackgroundColor = FLinearColor::White; //                     
        AngryButton->SynchronizeProperties();
    }

    if (HateButton)
    {
        HateButton->BackgroundColor = FLinearColor::White; //                     
        HateButton->SynchronizeProperties();
    }

    if (SurprisedButton)
    {
        SurprisedButton->BackgroundColor = FLinearColor::White; //                     
        SurprisedButton->SynchronizeProperties();
    }

    if (InterestButton)
    {
        InterestButton->BackgroundColor = FLinearColor::White; //                     
        InterestButton->SynchronizeProperties();
    }

    if (BoredButton)
    {
        BoredButton->BackgroundColor = FLinearColor::White; //                     
        BoredButton->SynchronizeProperties();
    }

    if (PainButton)
    {
        PainButton->BackgroundColor = FLinearColor::White; //                     
        PainButton->SynchronizeProperties();
    }

    if (bowButton)
    {
        bowButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        bowButton->SynchronizeProperties();
    }
    if (shakeButton)
    {
        shakeButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        shakeButton->SynchronizeProperties();
    }

    if (sorryButton)
    {
        sorryButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        sorryButton->SynchronizeProperties();
    }

    if (worryButton)
    {
        worryButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        worryButton->SynchronizeProperties();
    }

    if (shockButton)
    {
        shockButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        shockButton->SynchronizeProperties();
    }

    if (patButton)
    {
        patButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        patButton->SynchronizeProperties();
    }
    if (agreeingButton)
    {
        agreeingButton->BackgroundColor = FLinearColor::White; // 원래의 색상으로 복원
        agreeingButton->SynchronizeProperties();
    }

}

void UMyWidget::OnButtonClicked(UButton* ClickedButton)
{
    UE_LOG(LogTemp, Warning, TEXT("UMyWidget::OnButtonClicked is called."));
    if (APlayerController* PC = GetOwningPlayer())
    {
        UMyGameInstance* MyGameInstance = Cast<UMyGameInstance>(PC->GetGameInstance());
        if (MyGameInstance)
        {
            // 'ClickedButtonName'              Ŭ       ư    ̸           մϴ .
            FString ClickedButtonName = MyGameInstance->LastClickedButtonName;

            UE_LOG(LogTemp, Warning, TEXT("Button clicked: %s"), *ClickedButtonName);
        }
        else
        {
            UE_LOG(LogTemp, Warning, TEXT("MyGameInstance is null."));
        }
    }
}


void UMyWidget::SetSadButtonColor(FLinearColor Color)
{
    if (SadButton)
    {
        SadButton->BackgroundColor = Color;
        SadButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("SadButton is invalid."));
    }
}

void UMyWidget::SetJoyButtonColor(FLinearColor Color)
{
    if (JoyButton)
    {
        JoyButton->BackgroundColor = Color;
        JoyButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("JoyButton is invalid."));
    }
}

void UMyWidget::SetFearButtonColor(FLinearColor Color)
{
    if (FearButton)
    {
        FearButton->BackgroundColor = Color;
        FearButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("FearButton is invalid."));
    }
}

void UMyWidget::SetAngryButtonColor(FLinearColor Color)
{
    if (AngryButton)
    {
        AngryButton->BackgroundColor = Color;
        AngryButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("AngryButton is invalid."));
    }
}

void UMyWidget::SetHateButtonColor(FLinearColor Color)
{
    if (HateButton)
    {
        HateButton->BackgroundColor = Color;
        HateButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("HateButton is invalid."));
    }
}

void UMyWidget::SetSurprisedButtonColor(FLinearColor Color)
{
    if (SurprisedButton)
    {
        SurprisedButton->BackgroundColor = Color;
        SurprisedButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("SurprisedButton is invalid."));
    }
}

void UMyWidget::SetInterestButtonColor(FLinearColor Color)
{
    if (InterestButton)
    {
        InterestButton->BackgroundColor = Color;
        InterestButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("InterestButton is invalid."));
    }
}

void UMyWidget::SetBoredButtonColor(FLinearColor Color)
{
    if (BoredButton)
    {
        BoredButton->BackgroundColor = Color;
        BoredButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("BoredButton is invalid."));
    }
}

void UMyWidget::SetPainButtonColor(FLinearColor Color)
{
    if (PainButton)
    {
        PainButton->BackgroundColor = Color;
        PainButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("PainButton is invalid."));
    }
}

void UMyWidget::SetbowButtonColor(FLinearColor Color)
{
    if (bowButton)
    {
        bowButton->BackgroundColor = Color;
        bowButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("bowButton is invalid."));
    }
}

void UMyWidget::SetshakeButtonColor(FLinearColor Color)
{
    if (shakeButton)
    {
        shakeButton->BackgroundColor = Color;
        shakeButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("shakeButton is invalid."));
    }
}

void UMyWidget::SetsorryButtonColor(FLinearColor Color)
{
    if (sorryButton)
    {
        sorryButton->BackgroundColor = Color;
        sorryButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("sorryButton is invalid."));
    }
}

void UMyWidget::SetworryButtonColor(FLinearColor Color)
{
    if (worryButton)
    {
        worryButton->BackgroundColor = Color;
        worryButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("worryButton is invalid."));
    }
}

void UMyWidget::SetshockButtonColor(FLinearColor Color)
{
    if (shockButton)
    {
        shockButton->BackgroundColor = Color;
        shockButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("shockButton is invalid."));
    }
}

void UMyWidget::SetpatButtonColor(FLinearColor Color)
{
    if (patButton)
    {
        patButton->BackgroundColor = Color;
        patButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("patButton is invalid."));
    }
}

void UMyWidget::SetagreeingButtonColor(FLinearColor Color)
{
    if (agreeingButton)
    {
        agreeingButton->BackgroundColor = Color;
        agreeingButton->SynchronizeProperties();
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("agreeingButton is invalid."));
    }
}

// Step 1: 메시지 추가
void UMyWidget::AppendMessageToChatLog(const FString& Name, const FString& Message, bool bAddToChatLogs)
{


    if (!Chatlog)
    {
        UE_LOG(LogTemp, Error, TEXT("Chatlog is null!"));
        return;
    }

    UOverlay* TextOverlay = NewObject<UOverlay>(Chatlog);
    if (!TextOverlay)
    {
        UE_LOG(LogTemp, Error, TEXT("TextOverlay could not be created!"));
        return;
    }

    UVerticalBox* TextVerticalBox = NewObject<UVerticalBox>(TextOverlay);
    if (!TextVerticalBox)
    {
        UE_LOG(LogTemp, Error, TEXT("TextVerticalBox could not be created!"));
        return;
    }

    UTextBlock* UserNameTextBlock = NewObject<UTextBlock>(TextVerticalBox);
    if (!UserNameTextBlock)
    {
        UE_LOG(LogTemp, Error, TEXT("UserNameTextBlock could not be created!"));
        return;
    }

    UImage* UserChatBubble = NewObject<UImage>(TextOverlay);
    if (!UserChatBubble)
    {
        UE_LOG(LogTemp, Error, TEXT("UserChatBubble could not be created!"));
        return;
    }

    UTextBlock* UserChatTextBlock = NewObject<UTextBlock>(TextVerticalBox);
    if (!UserChatTextBlock)
    {
        UE_LOG(LogTemp, Error, TEXT("UserChatTextBlock could not be created!"));
        return;
    }

    if (bAddToChatLogs)
    {
        FChatLog NewChatLog;
        NewChatLog.CharacterName = Name;
        NewChatLog.Message = Message;
        NewChatLog.ChatImage = LoadObject<UTexture2D>(nullptr, TEXT("/Game/Chat/Image/Edit_chatbox.Edit_chatbox"));
        ChatLogs.Add(NewChatLog);
    }


    GetWorld()->GetTimerManager().SetTimer(DesiredSizeDelayHandle, [=]() {
        FSlateBrush Brush = UserChatBubble->Brush;

        // Set the image size to the desired size of the text block
        FVector2D DesiredSize = UserChatTextBlock->GetDesiredSize();
        Brush.ImageSize = DesiredSize;
        UserChatBubble->SetBrush(Brush);

        // 메시지 추가가 완료되면 다음 메시지를 처리하도록 알립니다.
        //OnMessageAdded();
        }, 0.5f, false);


    UserNameTextBlock->SetText(FText::FromString(Name));
    UserChatTextBlock->SetText(FText::FromString(Message));

    FSlateBrush& Brush = UserChatBubble->Brush;
    Brush.SetResourceObject(TestChatListInstance->TextBall);

    // Set the text color to black
    UserChatTextBlock->SetColorAndOpacity(FSlateColor(FLinearColor::Black));

    // Load custom font from the provided path
    UFont* CustomFont = Cast<UFont>(StaticLoadObject(UFont::StaticClass(), nullptr, TEXT("/Game/UI_image/LINESeedKR-Bd_Font")));

    if (CustomFont)
    {
        const FSlateFontInfo ChatFontInfo = FSlateFontInfo(CustomFont, 12);  // Chat text font
        const FSlateFontInfo NameFontInfo = FSlateFontInfo(CustomFont, 20);  // User name font
        UserChatTextBlock->SetFont(ChatFontInfo);
        UserNameTextBlock->SetFont(NameFontInfo);
    }

    // 이미지를 설정합니다. ChatImage가 null이 아니면 ChatImage를 사용하고, null이면 기본 텍스처를 사용합니다.
    UTexture2D* Texture = LoadObject<UTexture2D>(nullptr, TEXT("/Game/Chat/Image/Edit_chatbox.Edit_chatbox"));
    if (!Texture)
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to load Edit_chatbox texture!"));
        return;
    }
    Brush.SetResourceObject(Texture);

    // 이미지 크기를 텍스트 블록의 크기에 맞게 설정합니다.
    const float ImageSizeMultiplier = 2.0f;
    FVector2D DesiredSize = UserChatTextBlock->GetDesiredSize() * ImageSizeMultiplier;

    Brush.ImageSize = DesiredSize;
    UserChatBubble->SetBrush(Brush);
    UserChatBubble->SetOpacity(0.5f);


    UVerticalBoxSlot* UserNameSlot = TextVerticalBox->AddChildToVerticalBox(UserNameTextBlock);
    if (UserNameSlot)
    {
        UserNameSlot->SetPadding(FMargin(20.0f, 0.0f, 0.0f, 0.0f)); // 왼쪽에 20.0f의 패딩을 적용합니다.
    }

    UOverlaySlot* UserChatBubbleSlot = TextOverlay->AddChildToOverlay(UserChatBubble);
    if (UserChatBubbleSlot)
    {
        UserChatBubbleSlot->SetPadding(FMargin(40.0f, 40.0f, 0.0f, 0.0f)); // 왼쪽에 20.0f의 패딩을 적용합니다.
    }

    UVerticalBoxSlot* UserChatSlot = TextVerticalBox->AddChildToVerticalBox(UserChatTextBlock);
    if (UserChatSlot)
    {
        UserChatSlot->SetPadding(FMargin(20.0f, 0.0f, 0.0f, 0.0f)); // 왼쪽에 20.0f의 패딩을 적용합니다.
    }

    UOverlaySlot* TextVerticalBoxSlot = TextOverlay->AddChildToOverlay(TextVerticalBox);
    if (TextVerticalBoxSlot)
    {
        TextVerticalBoxSlot->SetPadding(FMargin(20.0f, 0.0f, 0.0f, 0.0f)); // 왼쪽에 20.0f의 패딩을 적용합니다.
    }

    Chatlog->AddChild(TextOverlay);
}


// Step 2: 저장
void UMyWidget::OnSaveBtnClicked()
{
    if (ChatLogs.Num() == 0)
    {
        UE_LOG(LogTemp, Warning, TEXT("No chat logs to save."));
        return;
    }
    float TotalPlaytime = 0 / 1000.0f; // 총 Playtime을 저장할 변수

    TArray<TSharedPtr<FJsonValue>> JsonValues;
    for (const auto& ChatLog : ChatLogs)
    {
        TSharedPtr<FJsonObject> JsonObject = MakeShareable(new FJsonObject);
        JsonObject->SetStringField("CharacterName", ChatLog.CharacterName);
        JsonObject->SetStringField("Message", ChatLog.Message);
        JsonObject->SetStringField("Action", ChatLog.Action);
        JsonObject->SetStringField("Playtime", ChatLog.Playtime);
        JsonObject->SetStringField("TTSUrl", ChatLog.TTSUrl);

        TSharedPtr<FJsonValueObject> JsonValue = MakeShareable(new FJsonValueObject(JsonObject));
        JsonValues.Add(JsonValue);
        TotalPlaytime += FCString::Atof(*ChatLog.Playtime); // 문자열을 float로 변환하여 합산

    }
    // 총 Playtime을 로그로 출력
    UE_LOG(LogTemp, Log, TEXT("Total Playtime while saving: %f milliseconds"), TotalPlaytime);
    BP_TotalPlaytime = TotalPlaytime;
    UE_LOG(LogTemp, Log, TEXT("bp_totalplaytime: %f seconds"), BP_TotalPlaytime);

    TSharedPtr<FJsonObject> FinalJsonObject = MakeShareable(new FJsonObject);
    FinalJsonObject->SetArrayField("ChatLogs", JsonValues);

    FString JsonString;
    TSharedRef<TJsonWriter<>> JsonWriter = TJsonWriterFactory<>::Create(&JsonString);
    FJsonSerializer::Serialize(FinalJsonObject.ToSharedRef(), JsonWriter);

    // 프로젝트 디렉토리 내에 "Saved/ChatLogs/" 경로에 "ChatLog.sav" 파일로 저장
    FString SavePath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Saved/ChatLogs/ChatLog.sav"));

    // 경로에 있는 폴더가 없으면 생성
    IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
    FString Directory = FPaths::GetPath(SavePath);
    if (!PlatformFile.DirectoryExists(*Directory))
    {
        PlatformFile.CreateDirectory(*Directory);
    }

    // 저장
    if (FFileHelper::SaveStringToFile(JsonString, *SavePath))
    {
        UE_LOG(LogTemp, Log, TEXT("Chat log saved successfully to: %s"), *SavePath);
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to save chat log to: %s"), *SavePath);
    }
}

// Step 3: 미리보기 불러오기
void UMyWidget::OnRecallBtnClicked()
{
    GetWorld()->GetTimerManager().ClearTimer(MessageAddingHandle);

    // 고정된 위치에서 ChatLog.sav 파일 경로 설정
    FString LoadPath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Saved/ChatLogs/ChatLog.sav"));

    FString JsonString;
    if (FFileHelper::LoadFileToString(JsonString, *LoadPath))
    {
        TSharedPtr<FJsonObject> JsonObject;
        TSharedRef<TJsonReader<>> JsonReader = TJsonReaderFactory<>::Create(JsonString);

        if (FJsonSerializer::Deserialize(JsonReader, JsonObject) && JsonObject.IsValid())
        {
            ChatLogs.Empty();

            const TArray<TSharedPtr<FJsonValue>> JsonValues = JsonObject->GetArrayField("ChatLogs");


            for (const TSharedPtr<FJsonValue>& Value : JsonValues)
            {
                const TSharedPtr<FJsonObject> LogObject = Value->AsObject();

                FChatLog ChatLog;
                ChatLog.CharacterName = LogObject->GetStringField("CharacterName");
                ChatLog.Message = LogObject->GetStringField("Message");
                if (LogObject->HasField("Emotion"))
                {
                    ChatLog.Action = LogObject->GetStringField("Emotion");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Emotion field missing in log index [%d]"), ChatLogs.Num());
                }
                if (LogObject->HasField("Action"))
                {
                    ChatLog.Action = LogObject->GetStringField("Action");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Action field missing in log index [%d]"), ChatLogs.Num());
                }

                if (LogObject->HasField("Playtime"))
                {
                    // Assuming Playtime is stored as string
                    ChatLog.Playtime = LogObject->GetStringField("Playtime");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Playtime field missing in log index [%d]"), ChatLogs.Num());
                }

                if (LogObject->HasField("TTSUrl"))
                {
                    ChatLog.TTSUrl = LogObject->GetStringField("TTSUrl");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("TTSUrl field missing in log index [%d]"), ChatLogs.Num());
                }

                // Playtime을 숫자로 변환하여 총 합계에 더함
                if (LogObject->HasField("Playtime"))
                {
                    ChatLog.Playtime = LogObject->GetStringField("Playtime");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Playtime field missing in log index [%d]"), ChatLogs.Num());
                }

                ChatLogs.Add(ChatLog);
            }


            CurrentLogIndex = 0;
            AddNextMessageFromLogs2();
        }
        else
        {
            UE_LOG(LogTemp, Error, TEXT("Failed to parse JSON."));
        }
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to load file from: %s"), *LoadPath);
    }
}

// Step 3_2: 녹화때 불러오기
void UMyWidget::OnRenderBtnClicked()
{
    GetWorld()->GetTimerManager().ClearTimer(MessageAddingHandle);

    // 고정된 위치에서 ChatLog.sav 파일 경로 설정
    FString LoadPath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Saved/ChatLogs/ChatLog.sav"));

    FString JsonString;
    if (FFileHelper::LoadFileToString(JsonString, *LoadPath))
    {
        TSharedPtr<FJsonObject> JsonObject;
        TSharedRef<TJsonReader<>> JsonReader = TJsonReaderFactory<>::Create(JsonString);

        if (FJsonSerializer::Deserialize(JsonReader, JsonObject) && JsonObject.IsValid())
        {
            ChatLogs.Empty();

            const TArray<TSharedPtr<FJsonValue>> JsonValues = JsonObject->GetArrayField("ChatLogs");


            for (const TSharedPtr<FJsonValue>& Value : JsonValues)
            {
                const TSharedPtr<FJsonObject> LogObject = Value->AsObject();

                FChatLog ChatLog;
                ChatLog.CharacterName = LogObject->GetStringField("CharacterName");
                ChatLog.Message = LogObject->GetStringField("Message");
                if (LogObject->HasField("Emotion"))
                {
                    ChatLog.Action = LogObject->GetStringField("Emotion");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Emotion field missing in log index [%d]"), ChatLogs.Num());
                }
                if (LogObject->HasField("Action"))
                {
                    ChatLog.Action = LogObject->GetStringField("Action");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Action field missing in log index [%d]"), ChatLogs.Num());
                }

                if (LogObject->HasField("Playtime"))
                {
                    // Assuming Playtime is stored as string
                    ChatLog.Playtime = LogObject->GetStringField("Playtime");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Playtime field missing in log index [%d]"), ChatLogs.Num());
                }

                if (LogObject->HasField("TTSUrl"))
                {
                    ChatLog.TTSUrl = LogObject->GetStringField("TTSUrl");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("TTSUrl field missing in log index [%d]"), ChatLogs.Num());
                }

                // Playtime을 숫자로 변환하여 총 합계에 더함
                if (LogObject->HasField("Playtime"))
                {
                    ChatLog.Playtime = LogObject->GetStringField("Playtime");
                }
                else
                {
                    UE_LOG(LogTemp, Warning, TEXT("Playtime field missing in log index [%d]"), ChatLogs.Num());
                }

                ChatLogs.Add(ChatLog);
            }


            CurrentLogIndex = 0;
            AddNextMessageFromLogs();
        }
        else
        {
            UE_LOG(LogTemp, Error, TEXT("Failed to parse JSON."));
        }
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to load file from: %s"), *LoadPath);
    }
}


// Step 4: 로그 재생
void UMyWidget::AddNextMessageFromLogs()
{
    UE_LOG(LogTemp, Warning, TEXT("Logging all Actions from ChatLogs:"));

    for (const auto& Log : ChatLogs)
    {
        UE_LOG(LogTemp, Warning, TEXT("Action: %s"), *Log.Action);
    }

    if (ChatLogs.IsValidIndex(CurrentLogIndex))
    {
        const auto& CurrentLog = ChatLogs[CurrentLogIndex];

        UE_LOG(LogTemp, Warning, TEXT("Processing Log Index: %d"), CurrentLogIndex);


        if (CurrentLogIndex % 2 == 0)
        {
            // 짝수번째 로그를 처리 중이면 CamA를 활성화합니다.
            ActivateCamA();
            UE_LOG(LogTemp, Warning, TEXT("Activated Camera A at Log Index: %d"), CurrentLogIndex);

        }
        else
        {
            // 홀수번째 로그를 처리 중이면 CamB를 활성화합니다.
            ActivateCamB();
            UE_LOG(LogTemp, Warning, TEXT("Activated Camera B at Log Index: %d"), CurrentLogIndex);

        }

        if (CurrentLog.Action.IsEmpty())
        {
            UE_LOG(LogTemp, Error, TEXT("Current Action is empty!"));
            // 에러 처리 코드 (예: 다음 인덱스로 넘어가기 또는 함수 종료)
            // 여기서 정확한 처리는 에러 상황에서 원하는 동작에 따라 달라집니다.
        }
        else
        {
            UE_LOG(LogTemp, Warning, TEXT("Current Action: %s"), *CurrentLog.Action);
        }

        AppendMessageToChatLog(CurrentLog.CharacterName, CurrentLog.Message, false);
        CheckAndPlayAnimationForKeyword(CurrentLog.Action);

        float PlaytimeFloat = FCString::Atof(*CurrentLog.Playtime) / 1000.0f;

        UE_LOG(LogTemp, Warning, TEXT("Playtime as float: %f"), PlaytimeFloat);




        CurrentLogIndex++;

        GetWorld()->GetTimerManager().SetTimer(MessageAddingHandle, this, &UMyWidget::AddNextMessageFromLogs, PlaytimeFloat, false);
    }
    else
    {
        GetWorld()->GetTimerManager().ClearTimer(MessageAddingHandle);
        UE_LOG(LogTemp, Warning, TEXT("End of ChatLogs, resetting CurrentLogIndex."));
        CurrentLogIndex = 0;
        FTimerHandle TimerHandle;
        GetWorld()->GetTimerManager().SetTimer(TimerHandle, this, &UMyWidget::RecordFinished, 2.0f, false);
        CurrentLogIndex = 0;
    }
}

// Step 4_2: 미리보기 로그 재생
void UMyWidget::AddNextMessageFromLogs2()
{
    UE_LOG(LogTemp, Warning, TEXT("Logging all Actions from ChatLogs:"));

    for (const auto& Log : ChatLogs)
    {
        UE_LOG(LogTemp, Warning, TEXT("Action: %s"), *Log.Action);
    }

    if (ChatLogs.IsValidIndex(CurrentLogIndex))
    {
        const auto& CurrentLog = ChatLogs[CurrentLogIndex];

        UE_LOG(LogTemp, Warning, TEXT("Processing Log Index: %d"), CurrentLogIndex);


        if (CurrentLogIndex % 2 == 0)
        {
            // 짝수번째 로그를 처리 중이면 CamA를 활성화합니다.
            ActivateCamA();
            UE_LOG(LogTemp, Warning, TEXT("Activated Camera A at Log Index: %d"), CurrentLogIndex);

        }
        else
        {
            // 홀수번째 로그를 처리 중이면 CamB를 활성화합니다.
            ActivateCamB();
            UE_LOG(LogTemp, Warning, TEXT("Activated Camera B at Log Index: %d"), CurrentLogIndex);

        }

        if (CurrentLog.Action.IsEmpty())
        {
            UE_LOG(LogTemp, Error, TEXT("Current Action is empty!"));
            // 에러 처리 코드 (예: 다음 인덱스로 넘어가기 또는 함수 종료)
            // 여기서 정확한 처리는 에러 상황에서 원하는 동작에 따라 달라집니다.
        }
        else
        {
            UE_LOG(LogTemp, Warning, TEXT("Current Action: %s"), *CurrentLog.Action);
        }

        AppendMessageToChatLog(CurrentLog.CharacterName, CurrentLog.Message, false);
        CheckAndPlayAnimationForKeyword(CurrentLog.Action);

        float PlaytimeFloat = FCString::Atof(*CurrentLog.Playtime) / 1000.0f;

        UE_LOG(LogTemp, Warning, TEXT("Playtime as float: %f"), PlaytimeFloat);




        CurrentLogIndex++;

        GetWorld()->GetTimerManager().SetTimer(MessageAddingHandle, this, &UMyWidget::AddNextMessageFromLogs2, PlaytimeFloat, false);
    }
    else
    {
        GetWorld()->GetTimerManager().ClearTimer(MessageAddingHandle);
        UE_LOG(LogTemp, Warning, TEXT("End of ChatLogs, resetting CurrentLogIndex."));
        CurrentLogIndex = 0;
    }
}



void UMyWidget::ActivateCamA()
{
    // CamA 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("Camera A has been activated!"));
    if (CamA)
    {
        // Programmatically "click" the button
        CamA->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("CamAButton is nullptr"));
    }
}

void UMyWidget::ActivateCamB()
{
    // CamB 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("Camera B has been activated!"));
    if (CamB)
    {
        // Programmatically "click" the button
        CamB->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("CamBButton is nullptr"));
    }
}

void UMyWidget::CheckAndPlayAnimationForKeyword(const FString& Action)
{
    UE_LOG(LogTemp, Warning, TEXT("Checking action: %s"), *Action);
    // Keyword와 관련된 애니메이션 로직을 별도의 함수로 분리하여 관리
    if (Action.Contains("bow", ESearchCase::IgnoreCase))
    {
        ActivateBowButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing bow animation"));
    }
    else if (Action.Contains("shake", ESearchCase::IgnoreCase))
    {
        ActivateShakeButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Shakce animation"));
    }
    else if (Action.Contains("sorry", ESearchCase::IgnoreCase))
    {
        ActivateSorryButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Sorry animation"));
    }

    else if (Action.Contains("Worry", ESearchCase::IgnoreCase))
    {
        ActivateWorryButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Worry animation"));
    }

    else if (Action.Contains("Shock", ESearchCase::IgnoreCase))
    {
        ActivateShockButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Shock animation"));
    }

    else if (Action.Contains("Pat", ESearchCase::IgnoreCase))
    {
        ActivatePatButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Pat animation"));
    }

    else if (Action.Contains("Agreeing", ESearchCase::IgnoreCase))
    {
        ActivateAgreeingButton();
        UE_LOG(LogTemp, Warning, TEXT("Playing Agreeing animation"));
    }

}



void UMyWidget::ActivateBowButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (bowButton)
    {
        // Programmatically "click" the button
        bowButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("BowButton is nullptr"));
    }
}
void UMyWidget::ActivateShakeButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (shakeButton)
    {
        // Programmatically "click" the button
        shakeButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("shakeButton is nullptr"));
    }
}
void UMyWidget::ActivateSorryButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (sorryButton)
    {
        // Programmatically "click" the button
        sorryButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("sorryButton is nullptr"));
    }
}
void UMyWidget::ActivateWorryButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (worryButton)
    {
        // Programmatically "click" the button
        worryButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("worryButton is nullptr"));
    }
}
void UMyWidget::ActivateShockButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (shockButton)
    {
        // Programmatically "click" the button
        shockButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("shockButton is nullptr"));
    }
}
void UMyWidget::ActivatePatButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (patButton)
    {
        // Programmatically "click" the button
        patButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("patButton is nullptr"));
    }
}
void UMyWidget::ActivateAgreeingButton()
{
    // BowButton 활성화 코드
    UE_LOG(LogTemp, Warning, TEXT("BowButton has been activated!"));
    if (agreeingButton)
    {
        // Programmatically "click" the button
        agreeingButton->OnClicked.Broadcast();
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("agreeingButton is nullptr"));
    }
}

void UMyWidget::UpdateLastChatLogAction(FString NewAction)
{
    if (ChatLogs.Num() > 0)
    {
        FChatLog& LastLog = ChatLogs.Last();
        if (LastLog.Action != NewAction)  // 변경될 경우만
        {
            LastLog.Action = NewAction;

            // 전체 배열 출력
            UE_LOG(LogTemp, Warning, TEXT("=== Start of ChatLogs ==="));
            for (const FChatLog& Log : ChatLogs)
            {
                UE_LOG(LogTemp, Warning, TEXT("Character: %s, Message: %s, Action: %s, Emotion: %s"),
                    *Log.CharacterName, *Log.Message, *Log.Action, *Log.Emotion);
            }
            UE_LOG(LogTemp, Warning, TEXT("=== End of ChatLogs ==="));
        }
    }
}

void UMyWidget::OnBowButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Bow Button Clicked!"));
    LastClickedAction = TEXT("bow");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnShakeButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Shake Button Clicked!"));
    LastClickedAction = TEXT("shake");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnSorryButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Sorry Button Clicked!"));
    LastClickedAction = TEXT("sorry");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnWorryButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Worry Button Clicked!"));
    LastClickedAction = TEXT("worry");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnShockButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Shock Button Clicked!"));
    LastClickedAction = TEXT("shock");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnPatButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Pat Button Clicked!"));
    LastClickedAction = TEXT("pat");
    UpdateLastChatLogAction(LastClickedAction);
}

void UMyWidget::OnAgreeingButtonClicked()
{
    UE_LOG(LogTemp, Warning, TEXT("Agreeing Button Clicked!"));
    LastClickedAction = TEXT("agreeing");
    UpdateLastChatLogAction(LastClickedAction);
}