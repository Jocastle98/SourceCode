#include "TestChatList.h"

void UTestChatList::NativeConstruct()
{
    Super::NativeConstruct();

    // Get a reference to the TextBall widget that was defined in Blueprint.
    TextBall = Cast<UImage>(GetWidgetFromName(TEXT("YourTextBallNameInBlueprint")));

    if (!TextBall)
    {
        UE_LOG(LogTemp, Error, TEXT("TextBall is null in UTestChatList::NativeConstruct"));
    }
}