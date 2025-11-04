// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/Image.h"
#include "Components/TextBlock.h"
#include "TestChatList.generated.h"

/**
 * 
 */
UCLASS()
class FRAMECAPTUREDEMO_API UTestChatList : public UUserWidget
{
	GENERATED_BODY()
	

public:
    UPROPERTY(BlueprintReadWrite, Category = "UI")
        UImage* TextBall;

    UPROPERTY(BlueprintReadWrite, Category = "UI")
        UTextBlock* ChatMsg;

    UPROPERTY(BlueprintReadWrite, Category = "UI")
        UTextBlock* SelectedNameText;

    virtual void NativeConstruct() override;
};
