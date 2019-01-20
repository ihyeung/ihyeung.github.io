 let url = "ws://" + location.host;
    var chatSocket = new WebSocket(url);
    chatSocket.onerror= function(error){
        console.log("Error. Current Chat socket ready state: " + chatSocket.readyState);
    };
    chatSocket.onopen = function(){
        chatSocket.send("join myroom");
        console.log("Socket open!");   
    };
   
    chatSocket.onmessage = function(message){
        setTimeout(echo, 5000);
        console.log(message.data);   
        document.getElementById("websocket").textContent += " MESSAGE RECEIVED: " + message.data;
    };

    let echo = function(){
        chatSocket.send("Hello, are you there?");
    };
    // chatSocket.onopen = document.getElementById("websocket").textContent = "SOCKET SUCCESS!";