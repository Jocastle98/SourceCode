#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/Button.h"
#include "Components/EditableText.h"
#include "Components/ComboBoxString.h"
#include "Components/ScrollBox.h"
#include "Components/TextBlock.h"
#include "Components/Overlay.h"
#include "Components/Image.h"
#include "TestChatList.h"
#include "JsonUtilities.h"
#include "Json.h"
#include "MyWidget.generated.h"


class AHttpPostActor;

USTRUCT(BlueprintType)
struct FChatLog
{
    GENERATED_BODY()

        UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString CharacterName;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString Message;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        UTexture2D* ChatImage = nullptr;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString Emotion;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString Action;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString TTSUrl;

    UPROPERTY(BlueprintReadWrite, EditAnywhere)
        FString Playtime;

    

};

UCLASS()
class FRAMECAPTUREDEMO_API UMyWidget : public UUserWidget
{
    GENERATED_BODY()

public:

    UFUNCTION(BlueprintImplementableEvent, Category = "WidgetEvents")
        void LoadingFinished();

    UFUNCTION(BlueprintImplementableEvent, Category = "WidgetEvents")
        void RecordFinished();
    
    UPROPERTY(BlueprintReadWrite, Category = "Chat")
        TArray<FChatLog> ChatLogs;

    UPROPERTY(BlueprintReadWrite, Category = "Chat")
        int32 CurrentLogIndex;

    UFUNCTION()
        void AddNextMessageFromLogs();

    UFUNCTION()
        void AddNextMessageFromLogs2();

    UFUNCTION(BlueprintCallable, Category = "Button Actions")
        void OnRecallBtnClicked();

    UFUNCTION(BlueprintCallable, Category = "Button Actions")
        void OnRenderBtnClicked();

    UFUNCTION(BlueprintCallable, Category = "Button Actions")
        void OnSaveBtnClicked();

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetSadButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetJoyButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetFearButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetAngryButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetHateButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetSurprisedButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetInterestButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetBoredButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetPainButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void ResetButtonColors();

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetbowButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetshakeButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetsorryButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetworryButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetshockButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetpatButtonColor(FLinearColor Color);

    UFUNCTION(BlueprintCallable, Category = "UI")
        void SetagreeingButtonColor(FLinearColor Color);

    UFUNCTION()
        void OnButtonClicked(UButton* ClickedButton);

    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
        UTextBlock* ResultText;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* SadButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* JoyButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* FearButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* AngryButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* HateButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* SurprisedButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* InterestButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* BoredButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* PainButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* bowButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* shakeButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* sorryButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* worryButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* shockButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* patButton;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* agreeingButton;

    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
        UEditableText* Chattxt;

    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
        UComboBoxString* namecombo;

    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
        UButton* Sendbtn;

    UPROPERTY(BlueprintReadWrite, meta = (BindWidget))
        UScrollBox* Chatlog;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* SaveBtn;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* RecallBtn;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* CamA;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* CamB;

    UPROPERTY(BlueprintReadOnly, meta = (BindWidget))
        UButton* RenderBtn;

    UPROPERTY(BlueprintReadOnly, Category = "Data")
        float BP_TotalPlaytime; // 이 변수는 블루프린트에서 읽을 수 있습니다.


    UMyWidget(const FObjectInitializer& ObjectInitializer);

    UPROPERTY()
        UTestChatList* TestChatListInstance;

    UFUNCTION(BlueprintImplementableEvent, Category = "Events")
        void OnSendButtonClickedBP();

    UFUNCTION(BlueprintCallable, Category = "File Operations")
        static void DeleteFilesInFolder(const FString& FolderPath);

    UFUNCTION()
        void AppendMessageToChatLog(const FString& Name, const FString& Message, bool bAddToChatLogs = true);

    void ActivateCamA(); // 함수 선언
    void ActivateCamB(); // 함수 선언
    void CheckAndPlayAnimationForKeyword(const FString& Action);
    void ActivateBowButton();
    void ActivateShakeButton();

    void ActivateSorryButton();
    void ActivateWorryButton();
    void ActivateShockButton();
    void ActivatePatButton();
    void ActivateAgreeingButton();
    void UpdateLastChatLogAction(FString NewAction);


    UFUNCTION()
        void OnBowButtonClicked();
    UFUNCTION()
        void OnShakeButtonClicked();
    UFUNCTION()
        void OnSorryButtonClicked();
    UFUNCTION()
        void OnWorryButtonClicked();
    UFUNCTION()
        void OnShockButtonClicked();
    UFUNCTION()
        void OnPatButtonClicked();
    UFUNCTION()
        void OnAgreeingButtonClicked();
    // 마지막으로 클릭된 액션을 저장합니다.
    UPROPERTY(BlueprintReadOnly)
        FString LastClickedAction;



protected:
    virtual void NativeConstruct() override;
    FTimerHandle MessageAddingHandle;
    FTimerHandle DesiredSizeDelayHandle;
private:
    AHttpPostActor* HttpPost;
};