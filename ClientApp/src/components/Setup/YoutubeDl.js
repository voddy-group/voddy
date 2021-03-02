import React, {useState} from "react";

export default function YoutubeDl() {
    const [youtubeDlStatus, setYoutubedlStatus] = useState("Click the button to test it.");
    const [hide, setHide] = useState(true);
    const [youtubeDlPath, setYoutubeDlPath] = useState("");

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
            setYoutubedlStatus("Working!")
        } else {
            setYoutubedlStatus("Broken! Error: " + returnedData.error)
            setHide(false);
        }
    }
    
    return (
        <div>
            <p>Test Youtube-dl installation:</p>
            <button onClick={TestYoutubeDl} />
            <p>Status: {youtubeDlStatus}</p>
            <div className={hide ? 'hidden' : ''}>
                <p>Make sure Youtube-dl is intalled, and test again.</p>
                <p>If you are sure you have Youtube-dl installed, entire the path here (e.g. "/usr/bin/youtube-dl" or "C:/youtube-dl/youtube-dl.exe":</p>
                <input onChange={handleChangeYoutubeDlPath}/>
                <p>Run the test again to verify the new path.</p>
            </div>
        </div>
    )
}