import React, {useState, useEffect} from "react";
import StreamerStreams from "./StreamerStreams";

export default function Streamer(match) {
    const [streamer, setStreamer] = useState({});
    const [streams, setStreams] = useState([]);
    
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
        GetStreamerStreams(response.data[0].streamId);
    }
    
    async function GetStreamerStreams(id) {
        const request = await fetch('backgroundTask/getStreams' +
            '?id=' + id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        
        setStreams(response.data);
    }
    
    async function DownloadStreams() {
        const request = await fetch('backgroundTask/downloadStreams',
            {
                Method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: streams
            });

        return await request.json();
    }
    
    // TODO needs performance checks; relies on other calls too much
    
    return (
        <div>
            <p>{streamer.displayName}</p>
            <img src={streamer.thumbnailLocation} />
            <button onClick={DownloadStreams}>Download vods</button>
            <table>
                <tbody>
                {streams.map(stream => <StreamerStreams key={stream.id} passedStream={stream} />)}
                </tbody>
            </table>
        </div>
    )
}