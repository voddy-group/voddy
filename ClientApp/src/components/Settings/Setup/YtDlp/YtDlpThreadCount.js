import React, {useEffect, useState} from "react";
import {Slider, Typography} from "@material-ui/core";
import {getCurrentSettings} from "../../General/General";

export default function YtDlpThreadCount() {
    const [maxThreads, setMaxThreads] = useState(0);
    const [currentThreads, setCurrentThreads] = useState(1);

    useEffect(() => {
        setGlobalSettings();
    }, [])

    async function setGlobalSettings() {
        var globalSettings = await getCurrentSettings();

        for (var x = 0; x < globalSettings.length; x++) {
            if (globalSettings[x].key == "workerCount") {
                setMaxThreads(JSON.parse(globalSettings[x].value).AvailableThreads);
            }

            if (globalSettings[x].key == "ytDlpThreadCount") {
                setCurrentThreads(globalSettings[x].value)
            }
        }
    }

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
            <Slider style={{width: "50%"}} max={maxThreads} min={1} marks value={currentThreads}
                    onChange={(e, value) => {
                        setCurrentThreads(value)
                    }} valueLabelDisplay={"auto"} onChangeCommitted={onChangeUpdateThreadCount}/>
        </div>
    )
}