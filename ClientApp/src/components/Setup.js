import React, { useState } from "react";

export default function Setup() {
    const [clientId, setClientId] = useState("");
    const [clientSecret, setClientSecret] = useState("");
    const [hideCredentialResult, setHideCredentialResult] = useState(true);
    const [credentialTest, setCredentialTest] = useState("");

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
            setHideCredentialResult(true);
            setCredentialTest("Recieved credentials; testing them...");
        }
    }
    
    return (
        <div>
            <h1>Setup page</h1>
            <p>Please enter your client ID:</p><input onChange={handleChangeClientId} defaultValue={clientId}/>
            <p>Please enter your client secret:</p><input onChange={handleChangeClientSecret} defaultValue={clientSecret}/>
            <button type="submit" onClick={backendTestCredentials}>Go</button>
            <p className={hideCredentialResult ? 'hidden': ''}>{credentialTest}</p>
        </div>
    )
}