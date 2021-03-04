import React, {useState, useEffect} from "react";

export default function StreamersRows(passedStreamer) {
    const [streamer] = useState(passedStreamer.passedStreamer);
    const [streamerUrl] = useState("/streamer/" + streamer.id)

    return (
        <tr>
            <a href={streamerUrl}>
            <td><img className={'thumbnail'} alt="thumbnail" src={streamer.thumbnailLocation}/></td>
            <td>{streamer.displayName}</td>
            </a>
        </tr>
    )
}