import React, {useState, useEffect} from "react";
import StreamersRows from "./StreamersRows";


export default function Streamers() {
    const [streamers, setStreamers] = useState([])
    
    useEffect(() => {
        getStreamers();
    }, [])
    
    async function getStreamers() {
        const request = await fetch('database/streamers',
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        
        setStreamers(response.data);
    }
    
    return (
        <div>
            <table>
                <tbody>
                {streamers.map(streamer => <StreamersRows key={streamer.id} passedStreamer={streamer} />)}
                </tbody>
            </table>
        </div>
    )
}