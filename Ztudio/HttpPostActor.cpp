#include "HttpPostActor.h"
#include "Json.h"
#include "HttpModule.h"
#include "MyGameInstance.h"

AHttpPostActor::AHttpPostActor()
{
    PrimaryActorTick.bCanEverTick = false;
}

void AHttpPostActor::BeginPlay()
{
    Super::BeginPlay();

    if (!MyWidgetClass)
    {
        return;
    }

    MyWidget = CreateWidget<UMyWidget>(GetWorld(), MyWidgetClass);
    if (!MyWidget)
    {
        return;
    }

    if (MyWidget) {
        UE_LOG(LogTemp, Warning, TEXT("MyWidget is valid."));
    }
    else {
        UE_LOG(LogTemp, Warning, TEXT("MyWidget is NOT valid."));
    }

    MyWidget->AddToViewport();

    if (MyWidget->Sendbtn)
    {
        MyWidget->Sendbtn->OnClicked.AddDynamic(this, &AHttpPostActor::OnSendButtonClicked);
        UE_LOG(LogTemp, Warning, TEXT("Sendbtn is valid."));
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("Sendbtn is not valid."));
    }

    APlayerController* PlayerController = GetWorld()->GetFirstPlayerController();
    if (PlayerController)
    {
        // 마우스 커서를 표시하고, UI 요소를 조작할 수 있도록 설정합니다.
        PlayerController->bShowMouseCursor = true;
        PlayerController->SetInputMode(FInputModeUIOnly());
    }
}

void AHttpPostActor::OnSendButtonClicked()
{

    UE_LOG(LogTemp, Warning, TEXT("OnSendButtonClicked() is called."));

    if (!MyWidget)
    {
        return;
    }

    if (!MyWidget->Chattxt || !MyWidget->namecombo)
    {
        return;
    }



    FHttpModule* Http = &FHttpModule::Get();
    TSharedRef<IHttpRequest, ESPMode::ThreadSafe> Request = Http->CreateRequest();

    UE_LOG(LogTemp, Warning, TEXT("Sending HTTP request..."));

    Request->SetVerb("POST");
    Request->SetURL("http://34.64.33.204:5000/init");
    Request->SetHeader("Content-Type", "application/json");



    UMyGameInstance* MyGameInstance = Cast<UMyGameInstance>(GetGameInstance());
    if (MyGameInstance)
    {
        UE_LOG(LogTemp, Warning, TEXT("Game instance casting succeeded."));


        FString name = MyWidget->namecombo->GetSelectedOption();
        FString chat = MyWidget->Chattxt->GetText().ToString();
        int32 SelectedIndex = MyWidget->namecombo->GetSelectedIndex();
        FString voice;

        if (SelectedIndex == 0) {
            voice = "nbora";
        }
        else if (SelectedIndex == 1) {
            voice = "njihwan";
        }

        // Iterate over the CharacterTTSMap to print its content
        for (const auto& Elem : MyGameInstance->CharacterTTSMap)
        {
            UE_LOG(LogTemp, Warning, TEXT("Key: %s, Value: %s"), *Elem.Key, *Elem.Value);
        }

        // If the name selected from the combo box is in the map, get the corresponding TTS value
        //if (MyGameInstance->CharacterTTSMap.Contains(name))
        //{
        //    voice = MyGameInstance->CharacterTTSMap[name];
        //    UE_LOG(LogTemp, Warning, TEXT("Selected TTS: %s"), *voice);  // Log the selected TTS
        //}
        //else
        //{
        //    UE_LOG(LogTemp, Warning, TEXT("The name '%s' is not found in the map."), *name);
        //}


        MyWidget->AppendMessageToChatLog(name, chat);

        FString JsonPayload = FString::Printf(TEXT("{\"name\":\"%s\",\"chat\":\"%s\", \"voice\":\"%s\"}"), *name, *chat, *voice);

        Request->SetContentAsString(JsonPayload);
        UE_LOG(LogTemp, Warning, TEXT("JSON Payload: %s"), *JsonPayload);
        Request->OnProcessRequestComplete().BindUObject(this, &AHttpPostActor::OnResponseReceived);
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("Game instance casting failed."));
    }

    UE_LOG(LogTemp, Warning, TEXT("HTTP request has been sent."));

    if (!Request->ProcessRequest())
    {
        UE_LOG(LogTemp, Error, TEXT("Failed to send HTTP request."));
    }

    if (MyWidget)
    {
        MyWidget->ResetButtonColors();
    }

    if (MyWidget && MyWidget->Chattxt)
    {
        MyWidget->Chattxt->SetText(FText::GetEmpty());
    }
}


void AHttpPostActor::OnResponseReceived(FHttpRequestPtr Request, FHttpResponsePtr Response, bool bWasSuccessful)
{
    UE_LOG(LogTemp, Warning, TEXT("OnResponseReceived() is called."));
    if (bWasSuccessful)
    {
        UE_LOG(LogTemp, Warning, TEXT("HTTP Request Successful!"));
        FString ResponseString = Response->GetContentAsString();
        UE_LOG(LogTemp, Warning, TEXT("Server response: %s"), *ResponseString);

        TSharedPtr<FJsonObject> JsonObject;
        TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(ResponseString);

        if (FJsonSerializer::Deserialize(Reader, JsonObject) && JsonObject.IsValid())
        {
            // 캐릭터 이름과 메시지를 추출합니다.
            FString CharacterName = JsonObject->GetStringField(TEXT("characterName"));
            FString Message = JsonObject->GetStringField(TEXT("message"));

            // 로그 배열에서 기존 로그를 찾습니다.
            FChatLog* ExistingLog = MyWidget->ChatLogs.FindByPredicate([&](const FChatLog& Log) {
                return Log.CharacterName == CharacterName && Log.Message == Message;
                });

            FChatLog NewChatLog;  // 변수를 if-else 외부로 옮깁니다.

            if (ExistingLog)
            {
                // 기존 로그가 있다면, 그 로그를 업데이트 합니다.
                ExistingLog->Emotion = JsonObject->GetStringField(TEXT("emotion"));
                ExistingLog->Action = JsonObject->GetStringField(TEXT("action"));
                ExistingLog->TTSUrl = JsonObject->GetStringField(TEXT("voice"));
                ExistingLog->Playtime = JsonObject->GetStringField(TEXT("Playtime"));
            }
            else
            {
                // 기존 로그가 없다면 새로운 로그를 만들고, 필드를 설정합니다.
                NewChatLog.CharacterName = CharacterName;
                NewChatLog.Message = Message;
                NewChatLog.Emotion = JsonObject->GetStringField(TEXT("emotion"));
                NewChatLog.Action = JsonObject->GetStringField(TEXT("action"));
                NewChatLog.TTSUrl = JsonObject->GetStringField(TEXT("voice"));
                NewChatLog.Playtime = JsonObject->GetStringField(TEXT("Playtime"));




                // 채팅 로그 배열에 새 로그를 추가합니다.
                MyWidget->ChatLogs.Add(NewChatLog);

            }
            PrintChatLogs();
            FString SaveDirectory = FPaths::ProjectSavedDir() + TEXT("TTSUrls/");
            FString FileName = FString::Printf(TEXT("%d.wav"), FileIndex++); // 고유한 파일 이름 생성, 확장자를 .wav로

            // 경로가 존재하지 않으면, 디렉터리 생성
            IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
            if (!PlatformFile.DirectoryExists(*SaveDirectory))
            {
                PlatformFile.CreateDirectory(*SaveDirectory);
            }

            FString FilePath = SaveDirectory + FileName;

            // TTSUrl 값을 파일로 저장
            FString TTSUrl = JsonObject->GetStringField(TEXT("voice"));

            // HTTP 요청을 시작하여 TTSUrl로부터 데이터를 받아온 후, 파일로 저장
            TSharedRef<IHttpRequest, ESPMode::ThreadSafe> HttpRequest = FHttpModule::Get().CreateRequest();
            HttpRequest->SetVerb("GET");
            HttpRequest->SetURL(TTSUrl);
            HttpRequest->OnProcessRequestComplete().BindLambda([FilePath](FHttpRequestPtr Request, FHttpResponsePtr Response, bool bWasSuccessful)
                {
                    if (bWasSuccessful)
                    {
                        if (Response->GetContentType().Equals(TEXT("audio/wav")) || Response->GetContentType().Equals(TEXT("application/octet-stream")))
                        {
                            TArray<uint8> Content;
                            Content = Response->GetContent();

                            if (FFileHelper::SaveArrayToFile(Content, *FilePath))
                            {
                                UE_LOG(LogTemp, Warning, TEXT("TTSUrl saved to file: %s"), *FilePath);
                            }
                            else
                            {
                                UE_LOG(LogTemp, Warning, TEXT("Failed to save TTSUrl to file: %s"), *FilePath);
                            }
                        }
                        else
                        {
                            UE_LOG(LogTemp, Warning,
                                TEXT("Unexpected content type in response. Expected 'audio/wav', got '%s'."),
                                *(Response->GetContentType()));
                        }
                    }
                });
            HttpRequest->ProcessRequest();


            // 로그 배열의 사이즈를 출력합니다.
            UE_LOG(LogTemp, Warning, TEXT("ChatLogs Count: %d"), MyWidget->ChatLogs.Num());


            FString EmotionToCheck = ExistingLog ? ExistingLog->Emotion : NewChatLog.Emotion;
            FString ActionToCheck = ExistingLog ? ExistingLog->Action : NewChatLog.Action;


            if (EmotionToCheck.Contains(TEXT("Sad")))
            {
                if (MyWidget && MyWidget->SadButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("SadButton is initialized. Calling SetSadButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Sad NewChatLog.Emotion Detected! Changing SadButton color..."));
                            MyWidget->SetSadButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Joy")))
            {
                if (MyWidget && MyWidget->JoyButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("SadButton is initialized. Calling SetJoyButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Joy NewChatLog.Emotion Detected! Changing JoyButton color..."));
                            MyWidget->SetJoyButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Fear")))
            {
                if (MyWidget && MyWidget->FearButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("FearButton is initialized. Calling SetFearButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Fear NewChatLog.Emotion Detected! Changing FearButton color..."));
                            MyWidget->SetFearButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Angry")))
            {
                if (MyWidget && MyWidget->AngryButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("AngryButton is initialized. Calling SetAngryButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Angry NewChatLog.Emotion Detected! Changing AngryButton color..."));
                            MyWidget->SetAngryButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Hate")))
            {
                if (MyWidget && MyWidget->HateButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("HateButton is initialized. Calling SetHateButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("HateNewChatLog.Emotion Detected! Changing HateButton color..."));
                            MyWidget->SetHateButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Surprised")))
            {
                if (MyWidget && MyWidget->SurprisedButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("SurprisedButton is initialized. Calling SetSurprisedButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Surprised NewChatLog.Emotion Detected! Changing SurprisedButton color..."));
                            MyWidget->SetSurprisedButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Interest")))
            {
                if (MyWidget && MyWidget->InterestButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("InterestButton is initialized. Calling SetInterestButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Interest NewChatLog.Emotion Detected! Changing InterestButton color..."));
                            MyWidget->SetInterestButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Bored")))
            {
                if (MyWidget && MyWidget->BoredButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("BoredButton is initialized. Calling SetBoredButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Bored NewChatLog.Emotion Detected! Changing BoredButton color..."));
                            MyWidget->SetBoredButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (EmotionToCheck.Contains(TEXT("Pain")))
            {
                if (MyWidget && MyWidget->PainButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("PainButton is initialized. Calling SetPainButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("Pain NewChatLog.Emotion Detected! Changing PainButton color..."));
                            MyWidget->SetPainButtonColor(FLinearColor::Blue);
                        });
                }
            }


            if (ActionToCheck.Contains(TEXT("bow")))
            {
                if (MyWidget && MyWidget->bowButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("bowButton is initialized. Calling SetbowButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("bow action Detected! Changing bowButton color..."));
                            MyWidget->SetbowButtonColor(FLinearColor::Blue);
                        });
                }
            }


            if (ActionToCheck.Contains(TEXT("shake")))
            {
                if (MyWidget && MyWidget->shakeButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("shakeButton is initialized. Calling SetshakeButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("shake action Detected! Changing shakeButton color..."));
                            MyWidget->SetshakeButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (ActionToCheck.Contains(TEXT("sorry")))
            {
                if (MyWidget && MyWidget->sorryButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("sorryButton is initialized. Calling SetsorryButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("sorry action Detected! Changing sorryButton color..."));
                            MyWidget->SetsorryButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (ActionToCheck.Contains(TEXT("worry")))
            {
                if (MyWidget && MyWidget->worryButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("worryButton is initialized. Calling SetworryButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("worry action Detected! Changing worryButton color..."));
                            MyWidget->SetworryButtonColor(FLinearColor::Blue);
                        });
                }
            }

            if (ActionToCheck.Contains(TEXT("shock")))
            {
                if (MyWidget && MyWidget->shockButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("shockButton is initialized. Calling SetshockButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("shock action Detected! Changing shockButton color..."));
                            MyWidget->SetshockButtonColor(FLinearColor::Blue);
                        });
                }


            }

            if (ActionToCheck.Contains(TEXT("pat")))
            {
                if (MyWidget && MyWidget->patButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("patButton is initialized. Calling SetpatButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("pat action Detected! Changing patButton color..."));
                            MyWidget->SetpatButtonColor(FLinearColor::Blue);
                        });
                }


            }

            if (ActionToCheck.Contains(TEXT("agreeing")))
            {
                if (MyWidget && MyWidget->agreeingButton)
                {
                    UE_LOG(LogTemp, Warning, TEXT("agreeingButton is initialized. Calling SetagreeingButtonColor()..."));


                    AsyncTask(ENamedThreads::GameThread, [this]()
                        {
                            GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Red, TEXT("agreeing action Detected! Changing agreeingButton color..."));
                            MyWidget->SetagreeingButtonColor(FLinearColor::Blue);
                        });
                }


            }

            if (MyWidget) // MyWidget이 유효한지 확인
            {
                MyWidget->LoadingFinished(); // 이벤트 호출
            }


        }
    }
    else
    {
        UE_LOG(LogTemp, Error, TEXT("HTTP Request failed."));
    }
}

void AHttpPostActor::PrintChatLogs()
{
    for (const FChatLog& ChatLog : MyWidget->ChatLogs)
    {
        UE_LOG(LogTemp, Warning, TEXT("CharacterName: %s"), *ChatLog.CharacterName);
        UE_LOG(LogTemp, Warning, TEXT("Message: %s"), *ChatLog.Message);
        UE_LOG(LogTemp, Warning, TEXT("Emotion: %s"), *ChatLog.Emotion);
        UE_LOG(LogTemp, Warning, TEXT("Action: %s"), *ChatLog.Action);
        UE_LOG(LogTemp, Warning, TEXT("TTSUrl: %s"), *ChatLog.TTSUrl);
        UE_LOG(LogTemp, Warning, TEXT("Playtime: %s"), *ChatLog.Playtime);

    }
}