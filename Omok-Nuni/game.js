const { compileClientWithDependenciesTracked } = require('pug');
const { v4: uuidv4 } = require('uuid');

module.exports = function(server) {
    const io = require('socket.io')(server);

    
    let rooms = [];
    let socketRooms = new Map();
    let userRequests = new Map();


    io.on('connection', (socket) => {
        console.log('[Socket.io] 사용자가 연결 되었습니다, socket.id=', socket.id);

        // 클라가 자신의 급수를 보내는 이벤트
        socket.on('setRank', (data) => {
            const myRank = data.myRank;

            // \+-1 범위의 랭크 방을 찾기 
            let index = rooms.findIndex(r => Math.abs(r.rank - myRank) <= 1);

            if (index !== -1) {
                // 이미 같은 rank로 만들어진 방이 있다면, 그 방에 입장
                let foundRoom = rooms.splice(index, 1)[0]; 
                let roomId = foundRoom.roomId;

                socket.join(roomId);
                socket.emit('joinRoom', { roomId: roomId });
                socket.to(roomId).emit('startGame', { roomId: roomId });
                socketRooms.set(socket.id, roomId);

            } else {
                //  해당 rank의 방이 없다면, 새 방 생성
                let newRoomId = uuidv4();
                socket.join(newRoomId);
                socket.emit('createRoom', { roomId: newRoomId });

                // rooms 배열에 rank 정보를 함께 push
                rooms.push({ roomId: newRoomId, rank: myRank });
                socketRooms.set(socket.id, newRoomId);

            }
        });

        // 방 나가기 
        socket.on('leaveRoom', (roomData) => {
            socket.leave(roomData.roomId);
            socket.emit('exitRoom');
            socket.to(roomData.roomId).emit('endGame');

            // 방 목록에서 해당 roomId 제거
            let roomId = socketRooms.get(socket.id);
            let idx = rooms.findIndex(r => r.roomId === roomId);
            if (idx !== -1) {
                rooms.splice(idx, 1);
            }
            socketRooms.delete(socket.id);
            userRequests.set(socket.id, roomId);
        });

        // 프로필을 다른 클라이언트에게 전달
        socket.on('opponentProfile', function(profileData) {
            const roomId = socketRooms.get(socket.id);
            if (roomId) {
                console.log('상대방 프로필 수신: ', profileData);
                
                // 방에 있는 모든 클라이언트에게 상대방 프로필 정보 전송
                socket.to(roomId).emit('opponentProfile', profileData);
            }
        });

        // 상대 유저가 둔 돌의 위치
        socket.on('doPlayer', function(moveData) {
            const roomId = moveData.roomId;
            const position = moveData.position;
            console.log('doPlayer 메시지를 받았습니다: ' + roomId + ' ' + position);
            socket.to(roomId).emit('doOpponent', { position: position });
        });

        // 재대국 요청 처리
        socket.on('sendRematchRequest', function() {
            console.log('클라로부터 재대국 요청을 받았습니다.');
            const roomId = socketRooms.get(socket.id);
            if (!roomId) return; // 사용자가 방에 없으면 처리하지 않음

            // 방에 있는 다른 클라이언트 찾기
            const roomClients = Array.from(io.sockets.adapter.rooms.get(roomId) || []);
            const otherSocketId = roomClients.find(id => id !== socket.id);

            if (otherSocketId) {
                // 상대방에게 재대국 요청 메시지 전송
                io.to(otherSocketId).emit('rematchRequestReceived', { roomId: roomId });
                console.log(`재대국 요청을 상대방(${otherSocketId})에게 보냈습니다.`);
            } else {
                // 상대방이 없으면 요청자에게 재대국 실패 알림
                io.to(socket.id).emit('rematchFailed');
                console.log(`재대국 요청 실패: 상대방이 없음.`);
            }
        });

        socket.on('rematchAccepted', function() {
            // 재대국 요청을 보낸 상대방에게 수락 메시지를 보냄
            const roomId = socketRooms.get(socket.id);
            if (!roomId) return;
        
            const roomClients = Array.from(io.sockets.adapter.rooms.get(roomId) || []);
            const requesterSocketId = roomClients.find(id => id !== socket.id);
        
            if (requesterSocketId) {
                io.to(requesterSocketId).emit('rematchAcceptedReceived');
                console.log(`재대국 요청을 보낸 사람(${requesterSocketId})에게 수락 메시지를 보냄.`);
            }
        });
        
        socket.on('startRematch', function() {
            // 기존 방 정보 가져오기
            const oldRoomId = socketRooms.get(socket.id);
            if (!oldRoomId) return;
        
            // 기존 방에 있는 클라이언트 정보 가져오기
            const roomClients = Array.from(io.sockets.adapter.rooms.get(oldRoomId) || []);
            const otherSocketId = roomClients.find(id => id !== socket.id);
        
            // 자신에게 'restartRoom' 메시지를 보냄
            socket.emit('restartRoom', { roomId: oldRoomId });
        
            if (otherSocketId) {
                // 상대방에게 'rejoinRoom' 메시지를 보냄
                io.to(otherSocketId).emit('joinRoom', { roomId: oldRoomId });
            }
        
            // 기존 방에서 게임을 리셋하고 새 게임 시작
            io.to(oldRoomId).emit('startGame', { roomId: oldRoomId });
        });

        socket.on('rematchRejected', function() {
            const roomId = socketRooms.get(socket.id);
            if (!roomId) return; // 사용자가 방에 없으면 처리하지 않음
        
            const roomClients = Array.from(io.sockets.adapter.rooms.get(roomId) || []);
            const otherSocketId = roomClients.find(id => id !== socket.id);
        
            if (otherSocketId) {
                io.to(otherSocketId).emit('rematchRejectedReceived'); // 상대방에게 보냄
                console.log(`재대국 요청이 거절됨. 상대방(${otherSocketId})에게 알림.`);
            }
        });

        // 기권 처리
        socket.on('sendForfeitRequest', function() {
            console.log('기권 요청을 받았습니다.');

            const roomId = socketRooms.get(socket.id);
            if (!roomId) return; // 방에 없는 경우 처리 안함

            const roomClients = Array.from(io.sockets.adapter.rooms.get(roomId) || []);
            const opponentSocketId = roomClients.find(id => id !== socket.id);

            if (opponentSocketId) {
                // 상대방에게 기권 처리 메시지 전송
                io.to(opponentSocketId).emit('forfeitWinReceived');
                console.log('상대방에게 기권 승리 메시지 전송.');
            }

            // 기권한 유저에게는 게임 종료 메시지 전송
            socket.emit('forfeitLoseReceived');
            console.log('나 자신에게 기권 패배 메시지 전송.');
        });

        // 연결 종료
        socket.on('disconnect', function() {
            console.log('사용자가 연결을 끊었습니다.');
        });
    });
}