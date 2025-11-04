import { getSafeAreaInsets, openGameCenterLeaderboard, submitGameCenterLeaderBoardScore } from '@apps-in-toss/web-framework';

/*
 * jslib 파일에서 호출될 전역 함수
 * Toss 웹 프레임워크의 getSafeAreaInsets 함수를 호출하고 결과를 Unity로 보냄
 * @param {string} gameObjectName - 결과를 받을 Unity GameObject 이름
 * @param {string} methodName - 결과를 받을 C# 함수 이름
 */
window.requestTossSafeArea = function(gameObjectName, methodName)
{
  // Unity로부터 요청을 받으면, 0.5초간 기다림
  // 이 시간 동안 브라우저는 CSS를 적용하고 캔버스 크기를 100%로 맞춤
  // Unity로 보낼 데이터 객체 (payload)
  setTimeout(() =>
  {
    let payload =
    {
      top: 0, bottom: 0, left: 0, right: 0,
      canvasWidth: 0, canvasHeight: 0
    };

      try
    {
      console.log("Toss 웹 프레임워크 감지. Safe Area 값 요청");
      const insets = getSafeAreaInsets();
      payload.top = insets.top;
      payload.bottom = insets.bottom;
      payload.left = insets.left;
      payload.right = insets.right;
    } 
    catch (e)
    {
      console.log("getSafeAreaInsets 호출 중 오류 발생: ", e);
    }

    // Uniyt 캔버스의 실제 렌더링 크기를 가져옴
    const canvas = document.querySelector("#unity-canvas");
    if (canvas)
    {
      // 0.5초 후, 안정화된 캔버스의 크기를 측정
      // clientWidth/clientHeight는 CSS에 의해 변경된 최종 크기를 나타냄
      payload.canvasWidth = canvas.clientWidth;
      payload.canvasHeight = canvas.clientHeight;
    }
    else
    {
      console.error("Unity 캔버스(#unity-canvas)를 찾을 수 없음");
    }

    // 완성된 데이터 객체를 JSON 문자열로 변환
    const payloadJson = JSON.stringify(payload);

    // Unity 인스턴스가 준비될 때까지 기다렸다가 데이터 전송
    const interval = setInterval(() =>
    {
      if (window.unityInstance)
      {
        clearInterval(interval);
        console.log(`Unity로 데이터 전송: ${payloadJson}`);
        window.unityInstance.SendMessage(gameObjectName, methodName, payloadJson);
      }
      else
      {
        console.error("Unity 인스턴스를 찾을 수 없습니다. index.html 설정을 확인해주세요.");
      }
    }, 100);
  }, 500);
};

window.openLeaderBoard = function () {
  openGameCenterLeaderboard();
};

window.submitScore = function (score) {
  submitGameCenterLeaderBoardScore({ score: String(score) });
};
