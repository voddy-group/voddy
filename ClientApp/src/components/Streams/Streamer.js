import React, {useState, useEffect} from "react";

export default function Streamer(match) {
    const [streamer, setStreamer] = useState({});
    
    useEffect(() => {
        GetStreamer();
    }, [])
    
    async function GetStreamer() {
        const request = await fetch('database/streamers' +
            '?id=' + match.match.params.id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        
        
        setStreamer(response.data[0]);
    }
    
    async function DownloadStreams() {
        const request = await fetch('backgroundTask/downloadStreams' +
            '?id=' + match.match.params.id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        return await request.json();
    }
    
    return (
        <div>
            <p>{streamer.displayName}</p>
            <img src={streamer.thumbnailLocation} />
            <button onClick={DownloadStreams} />
        </div>
    )
}