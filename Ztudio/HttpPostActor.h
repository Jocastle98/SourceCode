#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Runtime/Online/HTTP/Public/Http.h"
#include "MyWidget.h"
#include "HttpPostActor.generated.h"

UCLASS()
class FRAMECAPTUREDEMO_API AHttpPostActor : public AActor
{
    GENERATED_BODY()

public:
    AHttpPostActor();

    virtual void BeginPlay() override;

    void OnResponseReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bWasSuccessful);

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "UI")
        UMyWidget* MyWidget;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "UI")
        TSubclassOf<UMyWidget> MyWidgetClass;

    UFUNCTION(BlueprintCallable, Category = "HTTP")
        void OnSendButtonClicked();
    
    void PrintChatLogs();

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "HTTP")
        AHttpPostActor* HttpPost;

    int32 FileIndex = 1;

};