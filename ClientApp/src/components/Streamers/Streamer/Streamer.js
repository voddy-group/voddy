import React, {useState, useEffect} from "react";
import StreamerStreams from "./StreamerStreams";
import loading from "../../../assets/images/loading.gif";
import "../../../assets/styles/StreamSearch.css";

export default function Streamer(match) {
    const [streamer, setStreamer] = useState({});
    const [streams, setStreams] = useState([]);
    const [addButtonDisabled, setAddButtonDisabled] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [addButtontext, setAddButtonText] = useState("Download vods");
    const [addButtonClass, setAddButtonClass] = useState("add");

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
        GetStreamerStreams(response.data[0].streamerId);
    }
    
    async function GetStreamerStreams(id) {
        const request = await fetch('backgroundTask/getStreamsWithFilter' +
            '?id=' + id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        checkIfDownloaded(response.data);
        setStreams(response.data)
    }
    
    async function DownloadStreams() {
        var requestBody = {"data": streams}
        const request = await fetch('backgroundTask/downloadStreams',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody) //TODO dont like this
            });
        
        if (request.ok) {
            var newStreams = streams.slice();
            console.log(JSON.stringify(newStreams));
            for (var x = 0; x < newStreams.length; x++) {
                newStreams[x].alreadyAdded = true;
            }
            
            added();
            setStreams(newStreams);
        }
    }
    
    function checkIfDownloaded(newStreams) {
        var allDownloaded = false;
        for (var i = 0; i < newStreams.length; i++) {
            if (newStreams[i].alreadyAdded) {
                allDownloaded = true;
            } else {
                allDownloaded = false;
                break;
            }
        }
        if (allDownloaded) {
            added();
        }
    }

    function added() {
        setAddButtonText("All added!");
        setAddButtonClass("greyed")
        setIsLoading(false);
        setAddButtonDisabled(true);
    }
    
    // TODO needs performance checks; relies on other calls too much
    
    return (
        <div>
            <p>{streamer.displayName}</p>
            <img src={streamer.thumbnailLocation} />
            <button disabled={addButtonDisabled} className={addButtonClass} onClick={DownloadStreams}><img
                className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{addButtontext}</button>
            <table>
                <tbody>
                {streams.map(stream => <StreamerStreams key={stream.id} passedStream={stream} />)}
                </tbody>
            </table>
        </div>
    )
}