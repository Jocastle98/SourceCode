// MyGameInstance.cpp
#include "MyGameInstance.h"

UMyGameInstance::UMyGameInstance()
{
    // 슬롯 초기화
    Slot1CharacterName = TEXT("");
    Slot2CharacterName = TEXT("");
}

void UMyGameInstance::AddCharacterName(const FString& CharacterName)
{
    if (!CharacterName.IsEmpty())
    {
        CharacterNamesToBeAdded.AddUnique(CharacterName);
    }
}

TArray<FString> UMyGameInstance::GetAndClearCharacterNames()
{
    TArray<FString> Names = CharacterNamesToBeAdded;
    CharacterNamesToBeAdded.Empty();
    return Names;
}

void UMyGameInstance::SetSlot1CharacterName(const FString& NewName)
{
    // 이제 슬롯이 사전에 초기화되었으므로, 단순히 값을 할당합니다.
    if (!NewName.IsEmpty())
    {
        Slot1CharacterName = NewName;
        // 필요한 경우 여기서 콤보 박스를 업데이트하거나 다른 관련 작업을 수행할 수 있습니다.
    }
}

void UMyGameInstance::SetSlot2CharacterName(const FString& NewName)
{
    // 이제 슬롯이 사전에 초기화되었으므로, 단순히 값을 할당합니다.
    if (!NewName.IsEmpty())
    {
        Slot2CharacterName = NewName;
        // 필요한 경우 여기서 콤보 박스를 업데이트하거나 다른 관련 작업을 수행할 수 있습니다.
    }
}
