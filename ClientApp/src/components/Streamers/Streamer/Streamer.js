import React, {useState, useEffect} from "react";
import StreamerStreams from "./StreamerStreams";

export default function Streamer(match) {
    const [streamer, setStreamer] = useState({});
    const [streams, setStreams] = useState([]);
    
    var defaultStreams = [];
    var existingStreams = [];
    
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
        
        
        defaultStreams = response.data[0];
        setStreamer(response.data[0]);
        GetStreamerStreams(response.data[0].streamerId);
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
        defaultStreams = response.data;
        GetDownloadedStreams(id);
    }
    
    async function DownloadStreams() {
        const request = await fetch('backgroundTask/downloadStreams',
            {
                Method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: streams //TODO dont like this
            });

        return await request.json();
    }
    
    // TODO needs performance checks; relies on other calls too much
    
    async function GetDownloadedStreams(id) {
        const request = await fetch('database/streams' +
            '?streamerId=' + id,
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        
        existingStreams = response.data;
        
        ExcludeDownloadedStreams();
    }
    
    function ExcludeDownloadedStreams() {
        for (var x = 0; x < defaultStreams.length; x++) {
            for (var i = 0; i < existingStreams.length; i++) {
                if (parseInt(defaultStreams[x].id) === existingStreams[i].streamId) {
                    defaultStreams[x].alreadyAdded = true;
                    break;
                } else {
                    defaultStreams[x].alreadyAdded = false;
                }
            }
        }
        
        setStreams(defaultStreams);
    }
    
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