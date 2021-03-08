import React, {useState, useEffect} from "react";
import loading from "../../../assets/images/loading.gif";
import "../../../assets/styles/StreamSearch.css";

export default function StreamerStreams(passedStream) {
    const [stream, setStream] = useState(passedStream.passedStream);
    const [addButtonClass, setAddButtonClass] = useState("add");
    const [addButtonDisabled, setAddButtonDisabled] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [alreadyAdded, setAlreadyAdded] = useState(false);
    const [addButtontext, setAddButtonText] = useState("Add");


    if (passedStream.passedStream.alreadyAdded && !alreadyAdded) {
        added();
        setAlreadyAdded(true);
    }

    useEffect(() => {
        if (stream.thumbnail_url === "") {
            // TODO handle default image
        }
    }, [])

    function handleDownloadVodClick() {
        setIsLoading(true);
        setAddButtonText(null);

        downloadVod();
    }

    async function downloadVod() {
        const response = await fetch('backgroundTask/downloadStream',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(stream)
            });

        if (response.ok) {
            added();
        }
    }

    function added() {
        setAddButtonText("Added!");
        setAddButtonClass("greyed")
        setIsLoading(false);
        setAddButtonDisabled(true);
    }

    return (
        <tr>
            <td><img className={'thumbnail'} alt="thumbnail" src={stream.thumbnail_url}/></td>
            <td>{stream.title}</td>
            <td>{stream.view_count}</td>
            <td>{stream.duration}</td>
            <td>{new Date(stream.created_at).toLocaleString()}</td>
            <td>
                <button disabled={addButtonDisabled} className={addButtonClass} onClick={handleDownloadVodClick}><img
                    className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{addButtontext}</button>
            </td>
        </tr>
    )
}