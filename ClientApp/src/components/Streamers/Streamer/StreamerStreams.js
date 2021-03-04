import React, {useState, useEffect} from "react";

export default function StreamerStreams(passedStream) {
    const [stream, setStream] = useState(passedStream.passedStream);
    
    useEffect(() => {
        if (stream.thumbnail_url === "") {
            // TODO handle default image
        }
    }, [])
    
    async function handleDownloadVodClick() {
        const response = await fetch('backgroundTask/downloadStream',
            {
            method: 'post',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(stream)
        });

        const returnedData = await response.json();
    }
    
    return (
        <tr>
            <td><img className={'thumbnail'} alt="thumbnail" src={stream.thumbnail_url}/></td>
            <td>{stream.title}</td>
            <td>{stream.view_count}</td>
            <td>{stream.duration}</td>
            <td>{new Date(stream.created_at).toLocaleString()}</td>
            <td><button onClick={handleDownloadVodClick}>Download VOD</button></td>
        </tr>
    )
}