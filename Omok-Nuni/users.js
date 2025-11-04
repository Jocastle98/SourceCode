var express = require('express');
var router = express.Router();
var bcrypt = require('bcryptjs');
const { ObjectId } = require('mongodb');
var saltrounds = 10;

var ResponseType = 
{
  INVALID_USERNAME : 0,
  INVALID_PASSWORD : 1,
  SUCCESS: 2,
}

/* GET users listing. */
router.get('/', function(req, res, next) {
  res.send('respond with a resource');
});

// 회원가입
router.post('/signup', async function(req, res, next) {
  try {
    console.log('회원가입 요청이 왔습니다.');
    var username = req.body.username;
    var password = req.body.password;
    var nickname = req.body.nickname;
    var profileimageindex = req.body.profileimageindex;

    // 입력값 검증
    if (!username || !password || !nickname) {
      return res.status(400).send("모든 필드를 입력해주세요");
    }

    // 사용자 중복 체크
    var database = req.app.get('database');
    var users = database.collection('users');
   
    const existingUser = await users.findOne({ username: username });
    if (existingUser) {
      return res.status(409).send("이미 존재하는 사용자입니다");
    }
    // 비밀번호 암호화
    var salt = bcrypt.genSaltSync(saltrounds);
    var hash = bcrypt.hashSync(password, salt);
    // DB에 저장
    await users.insertOne({
      username: username,
      password: hash, // 해시된 비밀번호 저장
      nickname: nickname,
      profileimageindex: profileimageindex,
      coin: 500, // 기본 코인 제공 500개
      wincount: 0,
      losecount: 0,
      drawcount: 0,
      rank: 18,
      rankuppoints: 0,
      winlosestreak: 0,
      hasadremoval: false
    });
    res.status(201).send("사용자가 성공적으로 생성되었습니다");
  } catch (err) {
    console.error("사용자 추가 중 오류 발생:", err);
    res.status(500).send("서버 오류가 발생했습니다");
  }
});


// 로그인
router.post("/signin", async function(req, res, next) 
{
  try 
  {
    var username = req.body.username;
    var password = req.body.password;

    var database = req.app.get('database');
    var users = database.collection('users');

    // 입력값 검증
    if (!username || !password) 
    {
      return res.status(400).send("모든 필드를 입력해주세요");
    }

    const existingUser = await users.findOne({ username: username });
    if (existingUser) 
    {
      var compareResult = bcrypt.compareSync(password, existingUser.password);

      if (compareResult) 
      {
        req.session.isAuthenticated = true;
        req.session.userId = existingUser._id.toString();
        req.session.username = existingUser.username;
        req.session.nickname = existingUser.nickname;

        res.json(
        {
          result: ResponseType.SUCCESS,
          nickname: req.session.nickname
        });
      } 
      else 
      {
        res.json({result: ResponseType.INVALID_PASSWORD});
      }
    }
    else
    {
      res.json({result: ResponseType.INVALID_USERNAME});
    }
  }
  catch(err)
  {
    console.error("로그인 중 오류 발생:", err);
    res.status(500).send("서버 오류가 발생");
  }
});

// 자동 로그인
router.get('/autosignin', async function(req, res, next) {
  try {
    if (!req.session.isAuthenticated) {
      return res.status(403).send("로그인이 필요합니다.");
    }

    var userId = req.session.userId;
    var database = req.app.get('database');
    var users = database.collection('users');

    const user = await users.findOne({ _id: new ObjectId(userId) });

    if (!user) {
      return res.status(404).send("사용자를 찾을 수 없습니다.");
    }

    res.json({
      id: user._id.toString(),
      username: user.username,
      nickname: user.nickname,
      score: user.score || 0
    });
  } catch (err) {
    console.error("점수 조회 중 오류 발생: " + err);
    res.status(500).send("서버 오류가 발생했습니다.");
  }
});

// 로그아웃
router.post('/signout', function(req, res, next) 
{
  req.session.destroy((err) => 
  {
    if (err) 
    {
      console.error("세션 삭제 중 오류 발생:", err);
      return res.status(500).send("세션 삭제 중 오류가 발생했습니다");
    }
    res.status(200).send("로그아웃 되었습니다");
  });
});

// 사용자 정보 가져오기
router.get('/userinfo', async function(req, res, next) 
{
  try 
  {
    if (!req.session || !req.session.isAuthenticated) 
    {
      
      console.error("사용자 정보 가져오기 실패: 사용자 검증 실패");
      return res.status(400).send("사용자 검증 실패");
    }

    var database = req.app.get('database');
    var users = database.collection('users');

    const user = await users.findOne({ _id: new ObjectId(req.session.userId) });
    return res.status(200).json(
      {
        userId: user._id.toString(),
        username:           user.username,
        nickname:           user.nickname,
        profileimageindex:  user.profileimageindex,
        coin:               user.coin,
        wincount:           user.wincount,
        losecount:          user.losecount,
        drawcount:          user.drawcount,
        rank:               user.rank,
        rankuppoints:       user.rankuppoints,
        winlosestreak:      user.winlosestreak,
        hasadremoval:       user.hasadremoval
      });
  } 
  catch (err) 
  {
    console.error("사용자 정보 가져오기 중 오류 발생:", err);
    res.status(500).send("서버 오류가 발생했습니다");
  }
});

// 상대 정보 가져오기기
router.get('/userinfo/:targetUserId', async function(req, res) {
  try {
    if (!req.session || !req.session.isAuthenticated) {
      return res.status(400).send("사용자 검증 실패");
    }

    const targetUserId = req.params.targetUserId;
    const database = req.app.get('database');
    const users = database.collection('users');

    const user = await users.findOne({ _id: new ObjectId(targetUserId) });
    if (!user) {
      return res.status(404).send("해당 사용자를 찾을 수 없습니다.");
    }

    return res.status(200).json({
      username:           user.username,
      nickname:           user.nickname,
      profileimageindex:  user.profileimageindex,
      coin:               user.coin,
      wincount:           user.wincount,
      losecount:          user.losecount,
      drawcount:          user.drawcount,
      rank:               user.rank,
      rankuppoints:       user.rankuppoints,
      winlosestreak:      user.winlosestreak,
      hasadremoval:       user.hasadremoval
    });
  }
  catch(err) {
    console.error("특정 사용자 정보 가져오기 중 오류:", err);
    res.status(500).send("서버 오류");
  }
});

// 사용자 프로필 이미지 인덱스 수정
router.post('/changeprofileimage', async function(req, res, next) 
{
  try 
  {
    if (!req.session || !req.session.isAuthenticated) 
    {
      return res.status(400).send("사용자 검증 실패");
    }

    var userId = req.session.userId;
    var profileImageIndex = req.body.profileimageindex;
    var database = req.app.get('database');
    var users = database.collection('users');

    const result = await users.updateOne(
    { _id: ObjectId.createFromHexString(userId) },
    {
      $set: 
      {
        profileimageindex: Number(profileImageIndex),
        updateAt: new Date()
      }
    });

    if(result.matchedCount === 0)
    {
      return res.status(403).send("사용자를 찾을 수 없습니다.");
    }

    return res.sendStatus(200);
  }
  catch (err) 
  {
    console.error("프로필 이미지 변경 중 오류 발생: ", err);
    res.status(500).send("서버 오류가 발생했습니다.");
  }
});

// 사용자 승리 카운트 추가 및 등급 업데이트
router.post('/addwincount', async function(req, res, next) {
  if (!req.session || !req.session.isAuthenticated) {
    return res.status(400).send("사용자 검증 실패");
  }
  try {
    var database = req.app.get('database');
    var users = database.collection('users');
    
    // 현재 사용자 정보를 먼저 가져옴
    let user = await users.findOne({ _id: new ObjectId(req.session.userId) });
    
    // 승리 카운트, 승급 포인트, 연승 기록 업데이트
    const newWincount = user.wincount + 1;
    const newRankuppoints = user.rankuppoints + 1;  // 승리 시 승급 포인트 +1
    // 만약 이전 연승 기록이 음수였다면 1로, 아니면 +1 증가
    const newWinlosestreak = user.winlosestreak < 0 ? 1 : user.winlosestreak + 1;
    
    // 현재 등급에 따른 승급 기준 결정
    let threshold;
    if (user.rank >= 10) {
      threshold = 3; // 10급 ~ 18급: 3점
    } else if (user.rank >= 5) {
      threshold = 5; // 5급 ~ 9급: 5점
    } else {
      threshold = 10; // 1급 ~ 4급: 10점
    }
    
    let newRank = user.rank;
    let finalRankuppoints = newRankuppoints;
    // 승급 조건: 승급 포인트가 기준 이상이고, 이미 1급이 아니라면
    if (newRankuppoints >= threshold && user.rank > 1) {
      newRank = user.rank - 1; 
      finalRankuppoints = 0;   // 승급 후 승급 포인트 리셋
      console.log(`사용자 승급: ${user.rank}급에서 ${newRank}급으로 승급됨`);
    }
    
    await users.updateOne(
      { _id: new ObjectId(req.session.userId) },
      { 
        $set: { 
          wincount: newWincount, 
          rankuppoints: finalRankuppoints, 
          winlosestreak: newWinlosestreak, 
          rank: newRank 
        }
      }
    );
    
    res.status(200).send("승리 카운트 및 승급 포인트 업데이트 완료");
  } catch (err) {
    console.error("승리 카운트 업데이트 오류:", err);
    res.status(500).send("서버 오류");
  }
});


// 사용자 패배 카운트 추가 및 등급 업데이트targetUserId
router.post('/addlosecount', async function(req, res, next) {
  if (!req.session || !req.session.isAuthenticated) {
    return res.status(400).send("사용자 검증 실패");
  }
  try {
    var database = req.app.get('database');
    var users = database.collection('users');
    
    // 현재 사용자 정보를 먼저 가져옴
    let user = await users.findOne({ _id: new ObjectId(req.session.userId) });
    
    const newLosecount = user.losecount + 1;
    //  18급이면 -3 이하로 내려가지 않도록 하기
    let newRankuppoints = user.rankuppoints - 1;
    if (user.rank === 18 && newRankuppoints < -3) {
      newRankuppoints = -3;
    }
    // 만약 이전 연승 기록이 양수였다면 -1로, 아니면 -1씩 감소
    const newWinlosestreak = user.winlosestreak > 0 ? -1 : user.winlosestreak - 1;
    
    // 현재 급수에 따른 강등 필요 포인트(threshold) 결정
    let threshold;
    if (user.rank >= 10) {
      threshold = 3; // 10급 ~ 18급: 3점
    } else if (user.rank >= 5) {
      threshold = 5; // 5급 ~ 9급: 5점
    } else {
      threshold = 10; // 1급 ~ 4급: 10점
    }

    // 18급인 경우, rankuppoints가 -threshold 미만으로 내려가지 않도록
    if (user.rank === 18 && newRankuppoints < -threshold) {
      newRankuppoints = -threshold;
    }

    let newRank = user.rank;
    let finalRankuppoints = newRankuppoints;

    // 강등 조건: rankuppoints <= -threshold && 현재 급수가 18급이 아님
    if (newRankuppoints <= -threshold && user.rank < 18) {
      newRank = user.rank + 1;
      finalRankuppoints = 0; // 강등 후 rankuppoints 리셋
      console.log(`사용자 강등: ${user.rank}급 → ${newRank}급`);
    }
    
    await users.updateOne(
      { _id: new ObjectId(req.session.userId) },
      { 
        $set: { 
          losecount: newLosecount, 
          rankuppoints: finalRankuppoints, 
          winlosestreak: newWinlosestreak, 
          rank: newRank 
        }
      }
    );
    
    res.status(200).send("패배 카운트 및 승급 포인트 업데이트 완료");
  } catch (err) {
    console.error("패배 카운트 업데이트 오류:", err);
    res.status(500).send("서버 오류");
  }
});

// 오목 기보 저장 
router.post('/addomokrecord', async function(req, res) {
  if (!req.session || !req.session.isAuthenticated) {
    return res.status(400).send("사용자 검증 실패");
  }

  try {
    const { recordId, moves, blackUserId, whiteUserId } = req.body;

    
    if (!recordId || !moves) {
      return res.status(400).send("기보 데이터가 유효하지 않습니다.(recordId, moves 필수)");
    }

    
    const database = req.app.get('database');
    const omokrecords = database.collection('omokrecords');

    await omokrecords.updateOne(
      { recordId: recordId },
      {
        $set: {
          blackUserId: blackUserId || null,
          whiteUserId: whiteUserId || null,
          moves: moves,
          createdAt: new Date()
        }
      },
      { upsert: true }
    );

    return res.status(200).send("오목 기보 저장 완료");
  }
  catch(err) {
    console.error("오목 기보 저장 중 오류:", err);
    res.status(500).send("서버 오류");
  }
});




// 오목 기보 조회
router.get('/getomokrecords', async function(req, res) {
  if (!req.session || !req.session.isAuthenticated) {
    return res.status(400).send("사용자 검증 실패");
  }

  try {
    const database = req.app.get('database');
    const omokrecords = database.collection('omokrecords');
    const userId = req.session.userId;
    const userRecords = await omokrecords.find({
      $or: [
        { blackUserId: req.session.userId },
        { whiteUserId: req.session.userId }
      ]
    })
    .sort({ createdAt: -1 })
    .toArray();

    return res.status(200).json({
      records: userRecords
    });
  }
  catch(err) {
    console.error("오목 기보 조회 중 오류:", err);
    res.status(500).send("서버 오류");
  }
});

router.post('/addcoin', async function(req, res, next){
  if(!req.session || !req.session.isAuthenticated){
    return res.status(400).send("사용자 검증 실패");
  }

  try{
    var database = req.app.get('database');
    var users = database.collection('users');

    const coinAmount = parseInt(req.body.amount);

    if(isNaN(coinAmount) || coinAmount <=0){
      return res.status(400).send("유효하지 않은 코인 수량입니다");
    }

    let user = await users.findOne({_id: new ObjectId(req.session.userId)});

    const newCoinAmount = user.coin + coinAmount;

    await users.updateOne(
      {_id: new ObjectId(req.session.userId)},
      {$set: {coin: newCoinAmount}}
    );

    res.status(200).json({
      success: true,
      coin: newCoinAmount,
      message: '${coinAmount}코인이 추가되었습니다.'
    });
  } catch(err){
    console.error("코인 추가 중 오류 발생", err);
    res.status(500).send("서버 오류가 발생했습니다");
  }
});

router.post('/consumecoin', async function(req, res, next){
  if (!req.session || !req.session.isAuthenticated) {
    return res.status(400).send("사용자 검증 실패");
  }

  try {
    var database = req.app.get('database');
    var users = database.collection('users');

    const coinAmount = parseInt(req.body.amount);

    // 유효한 코인 수량인지 확인
    if (isNaN(coinAmount) || coinAmount <= 0) {
      return res.status(400).send("유효하지 않은 코인 수량입니다");
    }

    // 사용자 정보 가져오기
    let user = await users.findOne({ _id: new ObjectId(req.session.userId) });

    // 사용자가 충분한 코인을 가지고 있는지 확인
    if (user.coin < coinAmount) {
      return res.status(400).send("코인이 부족합니다. 필요한 코인을 확인해주세요.");
    }

    // 코인 차감
    const newCoinAmount = user.coin - coinAmount;

    // DB에서 코인 업데이트
    await users.updateOne(
      { _id: new ObjectId(req.session.userId) },
      { $set: { coin: newCoinAmount } }
    );

    res.status(200).json({
      success: true,
      coin: newCoinAmount,
      message: `${coinAmount} 코인이 차감되었습니다.`
    });
  } catch (err) {
    console.error("코인 소비 중 오류 발생", err);
    res.status(500).send("서버 오류가 발생했습니다.");
  }
});

router.post('/removeads', async function(req, res, next){
  if(!req.session || !req.session.isAuthenticated){
    return res.status(400).send("사용자 검증 실패");
  }

  try{
    var database = req.app.get('database');
    var users = database.collection('users');

    await users.updateOne(
      {_id: new ObjectId(req.session.userId)},
      {$set: {hasadremoval: true}}
    );

    res.status(200).send("광고 제거 적용 완료");
  } catch(err){
    console.error("광고 제거 업데이트 중 오류", err);
    res.status(500).send("서버 오류");
  }
})



module.exports = router;