// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/Button.h"
#include "Components/EditableText.h"
#include "TTS.generated.h"

/**
 * 
 */
UCLASS()
class FRAMECAPTUREDEMO_API UTTS : public UUserWidget
{
	GENERATED_BODY()
	
public:

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* nbora;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* njihwan;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* nminseo;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* nara;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* nes_c_hyeri;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* njaewook;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* njonghyun;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* neunwoo;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UEditableText* CharacterName;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* AddCharacterButton;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (BindWidget))
		UButton* AddCharacterButton2;



	UFUNCTION()
		void OnnboraButtonClicked();

	UFUNCTION()
		void OnnjihwanButtonClicked();

	UFUNCTION()
		void OnnminseoButtonClicked();

	UFUNCTION()
		void OnnaraButtonClicked();

	UFUNCTION()
		void Onnes_c_hyeriButtonClicked();

	UFUNCTION()
		void OnnjaewookButtonClicked();

	UFUNCTION()
		void OnnjonghyunButtonClicked();

	UFUNCTION()
		void OnneunwooButtonClicked();

	UFUNCTION()
		void OnAddCharacterButtonClicked();

	UFUNCTION()
		void OnAddCharacterButtonClicked2();

	virtual void NativeConstruct() override;
};
