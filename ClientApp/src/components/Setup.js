import React, {useState} from "react";

export default function Setup() {
    const [clientId, setClientId] = useState("");
    const [clientSecret, setClientSecret] = useState("");
    const [hideAuth, setHideAuth] = useState(true);
    const [hideAuthTokenRequest, setHideAuthTokenRequest] = useState(true);
    const [authUrl, setAuthUrl] = useState("");
    const [tokenVerefication, setTokenVerification] = useState("Getting Token...");

    let authWindow;

    function handleChangeClientId(e) {
        setClientId(e.target.value);
    }

    function handleChangeClientSecret(e) {
        setClientSecret(e.target.value);
    }

    function backendTestCredentials() {
        beginAuthProcess();
    }

    async function beginAuthProcess() {
        const sendData = {
            clientId: clientId,
            clientSecret: clientSecret
        }
        const response = await fetch('auth/twitchAuth', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(sendData)
        });
        const returnedData = await response.json();

        if (response.ok) {
            setHideAuth(false);
            setAuthUrl(returnedData.url);
            //openAuthUrl();
        }
    }

    function openAuthUrl() {
        authWindow = window.open(authUrl, 'mywindow', 'menubar=1,resizable=1,width=500,height=500');
        var id = setInterval(() => closeAuthUrl(id), 500);
    }

    function closeAuthUrl(id) {
        try {
            if (authWindow.location.href.indexOf("auth/twitchAuth/redirect") < 0) {
                console.log("safe");
            } else {
                clearInterval(id);
                authWindow.close();
                getToken();
            }
        } catch (e) {
            // nasty way of doing it, but it works. could not find an alternative
        }
    }
    
    async function getToken() {
        setHideAuthTokenRequest(false);

        const response = await fetch('auth/twitchAuth/token', {
            Method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const returnedData = await response.json();

        if (response.ok) {
            setTokenVerification("Success! You are now authenticated with twitch.");
        } else {
            setTokenVerification("Failed! Error: " + returnedData.error);
        }
    }

    return (
        <div>
            <h1>Setup page</h1>
            <p>Please enter your client ID:</p><input onChange={handleChangeClientId} defaultValue={clientId}/>
            <p>Please enter your client secret:</p><input onChange={handleChangeClientSecret}
                                                          defaultValue={clientSecret}/>
            <button type="submit" onClick={backendTestCredentials}>Go</button>
            <button className={hideAuth ? 'hidden' : ''} onClick={openAuthUrl}>Authenticate with twitch</button>
            <p className={hideAuthTokenRequest ? 'hidden' : ''}>{tokenVerefication}</p>
        </div>
    )
}