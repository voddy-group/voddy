import React, {useState, useEffect} from "react";

export default function StreamerGetLive(streamer) {

    function handleInputChange(e) {
        SetGetLive(e.target.checked)
    }

    async function SetGetLive(status) {
        const response = await fetch('streamer/' + streamer.streamerId + '/getLive?getLive=' + status,
            {
                method: 'put',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (!response.ok) {
            alert("Backend server not responding! Please refresh the page.");
        }
    }

    return (
        <div>
            <p>Download this streamers live streams?</p>
            <input type="checkbox" defaultChecked={streamer.getLive} onChange={handleInputChange}/>
            <span/>
        </div>
    )
}