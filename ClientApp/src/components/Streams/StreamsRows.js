import React, {useState, useEffect} from "react";

export default function StreamsRows(passedStreamer) {
    const [streamer] = useState(passedStreamer.passedStreamer);
    //constructor();
    
    
    function constructor() {
        streamer.thumbnailLocation = "/var/lib/voddy/" + streamer.thumbnailLocation;
    }

    return (
        <tr>
            <td><img className={'thumbnail'} alt="thumbnail" src={streamer.thumbnailLocation}/></td>
            <td>{streamer.displayName}</td>
        </tr>
    )
}