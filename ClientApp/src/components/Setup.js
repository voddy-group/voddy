import React, { useState } from "react";

export default function Setup() {
    const [clientId, setClientId] = useState("");
    const [clientSecret, setClientSecret] = useState("");
    const [hideAuth, setHideAuth] = useState(true);
    const [authUrl, setAuthUrl] = useState("");

    function handleChangeClientId(e) {
        setClientId(e.target.value);
    }
    
    function handleChangeClientSecret(e) {
        setClientSecret(e.target.value);
    }
    
    function backendTestCredentials() {
        test();
    }
    
    async function test() {
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
        }
    }
    
    function openAuthUrl() {
        window.open(authUrl, 'mywindow', 'menubar=1,resizable=1,width=500,height=500');
    }
    
    return (
        <div>
            <h1>Setup page</h1>
            <p>Please enter your client ID:</p><input onChange={handleChangeClientId} defaultValue={clientId}/>
            <p>Please enter your client secret:</p><input onChange={handleChangeClientSecret} defaultValue={clientSecret}/>
            <button type="submit" onClick={backendTestCredentials}>Go</button>
            <button className={hideAuth ? 'hidden': ''} onClick={openAuthUrl}>Authenticate with twitch</button>
        </div>
    )
}