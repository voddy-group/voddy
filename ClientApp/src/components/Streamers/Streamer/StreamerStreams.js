import React, {useState, useEffect} from "react";
import loading from "../../../assets/images/loading.gif";
import "../../../assets/styles/StreamSearch.css";
import cloneDeep from 'lodash/cloneDeep';

export default function StreamerStreams(passedStream) {
    const [stream, setStream] = useState(passedStream.passedStream);
    const [addButtonClass, setAddButtonClass] = useState("add");
    const [addButtonDisabled, setAddButtonDisabled] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [deleteIsLoading, setDeleteIsLoading] = useState(false);
    const [alreadyAdded, setAlreadyAdded] = useState(false);
    const [alreadyDeleted, setAlreadyDeleted] = useState(true);
    const [addButtontext, setAddButtonText] = useState("Add");
    const [deleteButtonText, setDeleteButtonText] = useState("Delete");
    const [hideDelete, setHideDelete] = useState(false);


    if (passedStream.passedStream.alreadyAdded && !alreadyAdded) {
        if (passedStream.passedStream.downloading) {
            added("Downloading...")
        }
        if (!passedStream.passedStream.downloading) {
            added("Downloaded!");
        }
        setAlreadyAdded(true);
    }

    useEffect(() => {
        var streamSize = stream.size;
        var newStream = cloneDeep(stream);
        if (((streamSize / 1024) / 1024) > 1000) {
            newStream.size = parseFloat(((streamSize / 1024) / 1024) / 1024).toFixed(2) + " GB";
        } else {
            newStream.size = parseFloat((streamSize / 1024) / 1024).toFixed(2) + " MB";
        }
        setStream(newStream);

        if (stream.thumbnail_url === "") {
            // TODO handle default image
        }
    }, [])

    function handleDownloadVodClick() {
        setIsLoading(true);
        setAddButtonText(null);

        downloadVod();
    }

    function handleDeleteClick() {
        setDeleteIsLoading(true);
        
        deleteVod();
    }

    async function downloadVod() {
        var removedSizeStream = cloneDeep(stream);
        delete removedSizeStream.size;
        const response = await fetch('backgroundTask/downloadStream',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(removedSizeStream)
            });

        if (response.ok) {
            added("Added!");
        }
    }

    async function deleteVod() {
        const response = await fetch('streams/deleteStream' +
            '?streamId=' + stream.id,
            {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (response.ok) {
            removed("Add")
        }
    }

    function added(customAddText) {
        setAddButtonText(customAddText);
        setAddButtonClass("greyed");
        setIsLoading(false);
        setAddButtonDisabled(true);
        setAlreadyDeleted(false);
    }

    function removed(customRemoveText) {
        setAddButtonText(customRemoveText);
        setAddButtonClass("add");
        setDeleteIsLoading(false);
        setHideDelete(true);
        setAddButtonDisabled(false);
        setAlreadyDeleted(true);
    }

    return (
        <tr>
            <td><img className={'thumbnail'} alt="thumbnail" src={stream.thumbnail_url}/></td>
            <td>{stream.title}</td>
            <td>{stream.view_count}</td>
            <td>{stream.duration}</td>
            <td>{new Date(stream.created_at).toLocaleString()}</td>
            <td>{stream.size}</td>
            <td>
                <button disabled={addButtonDisabled} className={addButtonClass} onClick={handleDownloadVodClick}><img
                    className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{addButtontext}</button>
            </td>
            <td>
                <button onClick={handleDeleteClick} disabled={alreadyDeleted}
                        className={alreadyDeleted ? 'hidden' : ''}><img
                    className={deleteIsLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>Delete
                </button>
            </td>
        </tr>
    )
}