import React, {useEffect, useState} from "react";
import {makeStyles, Switch, Typography} from "@material-ui/core";

export default function VideoThumbnails(props) {
    const [checked, setChecked] = useState(false);

    useEffect(() => {
        setChecked(props.generateVideoThumbnails);
    }, [props.generateVideoThumbnails])
    
    async function handleSwitchChange() {
        const request = await fetch('setup/globalSettings', {
            method: 'put',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "generationEnabled": !checked
            })
        });
        
        if (request.ok) {
            setChecked(!checked);
        }
    }

    return (
        <div>
            <h2>Generate Video Thumbnails</h2>
            <Typography variant={"body1"}>VOD video thumbnails can be generated when a VOD has been fully downloaded.
                This requires heavy CPU usage for a prolonged amount of time.</Typography>
            <Typography variant={"body1"}>Generate video thumbnails?<Switch color="primary" checked={checked}
                                                                            onChange={handleSwitchChange}/></Typography>
        </div>
    )
}