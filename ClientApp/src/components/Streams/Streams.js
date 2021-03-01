import React, {useState, useEffect} from "react";
import StreamsRows from "./StreamsRows";

export default function Streams() {
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
                {streamers.map(streamer => <StreamsRows key={streamer.id} passedStreamer={streamer} />)}
                </tbody>
            </table>
        </div>
    )
}