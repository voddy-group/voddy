import React, {useState, useEffect} from "react";
import { useHistory } from "react-router-dom";
import StreamerStreams from "./StreamerStreams";
import loading from "../../../assets/images/loading.gif";
import "../../../assets/styles/StreamSearch.css";
import StreamerStreamQuality from "./StreamerStreamQuality";
import cloneDeep from 'lodash/cloneDeep';

export default function Streamer(match) {
    const [streamer, setStreamer] = useState({});
    const [streams, setStreams] = useState([]);
    const [addButtonDisabled, setAddButtonDisabled] = useState(false);
    const [addIsLoading, setAddIsLoading] = useState(false);
    const [deleteIsLoading, setDeleteIsLoading] = useState(false);
    const [deleteAllisLoading, setDeleteAllIsLoading] = useState(false);
    const [addButtonText, setAddButtonText] = useState("Download vods");
    const [deleteButtonText, setDeleteButtonText] = useState("Delete Streamer");
    const [deleteAllButtonText, setDeleteAllButtonText] = useState("Delete All Streams");
    const [addButtonClass, setAddButtonClass] = useState("add");
    const [deleteButtonClass, setDeleteButtonClass] = useState("add");
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(false);
    
    let history = useHistory();

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
        const request = await fetch('streams/getStreamsWithFilter' +
            '?id=' + id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        
        /*for(var x in response.data) {
            response.data[x].alreadyAdded = true;
        }*/
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

            added();
            setStreams(setAdded(true));
        }
    }

    async function DeleteStreamer() {
        const request = await fetch('database/streamer' +
            '?streamerId=' + streamer.streamerId,
            {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
        
        if (request.ok) {
            deleted();
            setStreams(setAdded(true));
            history.goBack();
        }
    }

    function setAdded(added) {
        var newStreams = [...streams];
        for (var x = 0; x < newStreams.length; x++) {
            newStreams[x].alreadyAdded = added;
        }
        return newStreams;
    }

    async function handleDeleteClick() {
        setDeleteAllIsLoading(true);
        setDeleteAllButtonText("Deleting...");

        const request = await fetch('streams/deleteStreams' +
            '?streamerId=' + streamer.streamerId,
            {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                }
            })

        if (request.ok) {
            setDeleteAllIsLoading(false);
            setDeleteAllButtonText("Deleted!");
            
            setStreams(setAdded(false));
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
        setAddIsLoading(false);
        setAddButtonDisabled(true);
    }

    function deleted() {
        setAddButtonDisabled(true);
        setDeleteButtonDisabled(true);
        setAddButtonClass("greyed");
        setDeleteButtonClass("greyed");
        setAddIsLoading(true);
        setDeleteIsLoading(true);
        setDeleteButtonText("Deleting...");
    }

    // TODO needs performance checks; relies on other calls too much

    return (
        <div>
            <p>{streamer.displayName}</p>
            <img src={streamer.thumbnailLocation}/>
            <p>{streamer.isLive ? 'LIVE': ''}</p>
            <StreamerStreamQuality streamer={streamer} />
            <button disabled={addButtonDisabled} className={addButtonClass} onClick={DownloadStreams}><img
                className={addIsLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{addButtonText}</button>
            <button disabled={deleteButtonDisabled} className={deleteButtonClass} onClick={DeleteStreamer}><img
                className={deleteIsLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{deleteButtonText}
            </button>
            <button onClick={handleDeleteClick}><img
                className={deleteAllisLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{deleteAllButtonText}
            </button>
            <table>
                <tbody>
                {streams.map(stream => <StreamerStreams key={stream.id} passedStream={stream}/>)}
                </tbody>
            </table>
        </div>
    )
}