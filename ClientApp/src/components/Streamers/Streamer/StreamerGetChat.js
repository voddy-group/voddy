import React from "react";
import "../../../assets/styles/StreamSearch.css";

export default function StreamerGetChat(stream) {
    
    async function handleButtonClick() {
        const response = await fetch('chat/' + stream.id,
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (response.ok) {
            var jsonResponse = await response.json();
            const link = document.createElement('a');
            link.download = stream.id + ".txt";
            link.href = jsonResponse.url;
            document.body.append(link);
            link.click();
            document.body.removeChild(link);
        }
    }
    
    return (
        <div>
            <button onClick={handleButtonClick} disabled={!stream.downloaded}>Chat</button>
        </div>
    )
}
