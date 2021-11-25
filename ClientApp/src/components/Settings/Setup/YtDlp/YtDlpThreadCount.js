import React, {useEffect, useState} from "react";
import {Slider, Typography} from "@material-ui/core";
import {getCurrentSettings} from "../../General/General";

export default function YtDlpThreadCount(props) {
    const [maxThreads, setMaxThreads] = useState(props.maxThreads);
    const [currentThreads, setCurrentThreads] = useState(props.currentThreads);

    async function onChangeUpdateThreadCount(event, value) {
        const request = await fetch('setup/globalSettings', {
            method: 'put',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "ytDlpThreadCount": value
            })
        });

        if (request.ok) {
            setCurrentThreads(value);
        }
    }

    return (
        <div>
            <Typography variant="h5">Thread Count</Typography>
            <Typography>yt-dlp supports multi-threading downloads. You can increase your download speed at the cost of
                CPU usage. Do note that users with fast internet should be careful with this value, as allowing yt-dlp
                to use all rescources will max out your computers CPU.</Typography>
            <Typography>This value is not effected by the applications thread limit on the general settings
                area.</Typography>
            <Slider style={{width: "50%"}} max={props.maxThreads} min={1} marks value={currentThreads === 0 ? props.currentThreads : currentThreads}
                    onChange={(e, value) => {
                        setCurrentThreads(value)
                    }} valueLabelDisplay={"auto"} onChangeCommitted={onChangeUpdateThreadCount}/>
        </div>
    )
}