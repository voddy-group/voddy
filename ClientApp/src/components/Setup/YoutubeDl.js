import React, {useState} from "react";

export default function YoutubeDl() {
    const [youtubeDlStatus, setYoutubedlStatus] = useState("Click the button to test it.");
    const [hide, setHide] = useState(true);
    const [youtubeDlPath, setYoutubeDlPath] = useState("");
    const [updateButtonDisabled, setUpdateButtonDisabled] = useState(false);
    const [updateButtonText, setUpdateButtonText] = useState("Force Download/Update Youtube-dl")

    function handleChangeYoutubeDlPath(e) {
        setYoutubeDlPath(e.target.value);
    }
    
    async function TestYoutubeDl() {
        setYoutubedlStatus("Testing...");
        const response = await fetch('youtubeDl/test' +
            '?path=' + youtubeDlPath, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
        });

        const returnedData = await response.json();
        
        if (returnedData.error == null) {
            setYoutubedlStatus("Working!");
            setHide(true);
        } else {
            setYoutubedlStatus("Broken! Error: " + returnedData.error);
            setHide(false);
        }
    }
    
    async function ForceUpdateYoutubeDl() {
        setUpdateButtonText("Downloading...");
        setUpdateButtonDisabled(true);
        const response = await fetch('youtubeDl/update',
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        if (response.ok) {
            setUpdateButtonText("Updated!")
            setUpdateButtonDisabled(false);
        }
    }
    
    return (
        <div>
            <p>Test Youtube-dl installation:</p>
            <button onClick={TestYoutubeDl}>Test Youtube-dl</button>
            <p>Status: {youtubeDlStatus}</p>
            <button disabled={updateButtonDisabled} onClick={ForceUpdateYoutubeDl}>{updateButtonText}</button>
            <div className={hide ? 'hidden' : ''}>
                <p>Make sure Youtube-dl is intalled, and test again.</p>
                <p>If you are sure you have Youtube-dl installed, entire the path here (e.g. "/usr/bin/youtube-dl" or "C:/youtube-dl/youtube-dl.exe":</p>
                <input onChange={handleChangeYoutubeDlPath}/>
                <p>Run the test again to verify the new path.</p>
            </div>
        </div>
    )
}