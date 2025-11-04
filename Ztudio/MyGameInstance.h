// MyGameInstance.h

#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "MyGameInstance.generated.h"

/**
 * GameInstance 클래스는 게임 전반에 걸쳐 지속되는 데이터와 로직을 관리합니다.
 */
UCLASS()
class FRAMECAPTUREDEMO_API UMyGameInstance : public UGameInstance
{
    GENERATED_BODY()

public:
    // 생성자
    UMyGameInstance();

    // 마지막으로 클릭된 버튼의 이름을 저장합니다.
    UPROPERTY(BlueprintReadOnly, Category = "Button")
        FString LastClickedButtonName;

    // 캐릭터 이름을 저장할 TMap 입니다.
    UPROPERTY()
        TMap<FString, FString> CharacterTTSMap;

    // 캐릭터 슬롯 이름을 저장하는 변수입니다.
    UPROPERTY(BlueprintReadOnly, Category = "Character Slots")
        FString Slot1CharacterName;

    UPROPERTY(BlueprintReadOnly, Category = "Character Slots")
        FString Slot2CharacterName;

    // 캐릭터 이름을 배열에 추가하는 함수입니다.
    void AddCharacterName(const FString& CharacterName);

    // 캐릭터 이름 배열을 가져오고 초기화하는 함수입니다.
    TArray<FString> GetAndClearCharacterNames();

    // 슬롯 이름을 설정하는 함수입니다.
    void SetSlot1CharacterName(const FString& NewName);
    void SetSlot2CharacterName(const FString& NewName);

private:
    // 캐릭터 이름을 추가하기 위한 내부 배열입니다.
    TArray<FString> CharacterNamesToBeAdded;
};

