// Fill out your copyright notice in the Description page of Project Settings.
#include "TTS.h"
#include "MyGameInstance.h"


void UTTS::NativeConstruct()
{
    Super::NativeConstruct();

    nbora->OnClicked.AddDynamic(this, &UTTS::OnnboraButtonClicked);
    njaewook->OnClicked.AddDynamic(this, &UTTS::OnnjaewookButtonClicked);
    njonghyun->OnClicked.AddDynamic(this, &UTTS::OnnjonghyunButtonClicked);
    nminseo->OnClicked.AddDynamic(this, &UTTS::OnnminseoButtonClicked);
    nes_c_hyeri->OnClicked.AddDynamic(this, &UTTS::Onnes_c_hyeriButtonClicked);
    neunwoo->OnClicked.AddDynamic(this, &UTTS::OnneunwooButtonClicked);
    nara->OnClicked.AddDynamic(this, &UTTS::OnnaraButtonClicked);
    njihwan->OnClicked.AddDynamic(this, &UTTS::OnnjihwanButtonClicked);
    AddCharacterButton->OnClicked.AddDynamic(this, &UTTS::OnAddCharacterButtonClicked);
    AddCharacterButton2->OnClicked.AddDynamic(this, &UTTS::OnAddCharacterButtonClicked2);
}


void UTTS::OnnboraButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot1CharacterName(characterName);
        }
    }
}

void UTTS::OnnjihwanButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot2CharacterName(characterName);
        }
    }
}

void UTTS::OnnminseoButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot1CharacterName(characterName);
        }
    }
}

void UTTS::OnnaraButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot2CharacterName(characterName);
        }
    }
}

void UTTS::Onnes_c_hyeriButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot1CharacterName(characterName);
        }
    }
}

void UTTS::OnnjaewookButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot2CharacterName(characterName);
        }
    }
}

void UTTS::OnnjonghyunButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot1CharacterName(characterName);
        }
    }
}

void UTTS::OnneunwooButtonClicked()
{
    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (GameInstance && CharacterName)
    {
        FString characterName = CharacterName->GetText().ToString();
        if (!characterName.IsEmpty())
        {
            GameInstance->SetSlot2CharacterName(characterName);
        }
    }
}




void UTTS::OnAddCharacterButtonClicked()
{
    FString characterName = CharacterName->GetText().ToString();

    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (!GameInstance)
    {
        UE_LOG(LogTemp, Warning, TEXT("GameInstance is nullptr"));
        return;
    }

    if (CharacterName == nullptr)
    {
        UE_LOG(LogTemp, Warning, TEXT("CharacterName is nullptr"));
        return;
    }

    // 이제 nullptr이 아님을 확인했으므로, 실제 작업을 수행합니다.
    if (!characterName.IsEmpty())
    {
        GameInstance->SetSlot1CharacterName(characterName);
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("characterName is Empty"));
    }
}


void UTTS::OnAddCharacterButtonClicked2()
{
    FString characterName = CharacterName->GetText().ToString();

    UMyGameInstance* GameInstance = GetGameInstance<UMyGameInstance>();
    if (!GameInstance)
    {
        UE_LOG(LogTemp, Warning, TEXT("GameInstance is nullptr"));
        return;
    }

    if (CharacterName == nullptr)
    {
        UE_LOG(LogTemp, Warning, TEXT("CharacterName is nullptr"));
        return;
    }

    // 이제 nullptr이 아님을 확인했으므로, 실제 작업을 수행합니다.
    if (!characterName.IsEmpty())
    {
        GameInstance->SetSlot2CharacterName(characterName);
    }
    else
    {
        UE_LOG(LogTemp, Warning, TEXT("characterName is Empty"));
    }
}




