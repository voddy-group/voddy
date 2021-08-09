import React, {useEffect, useState} from "react";
import StreamQuality from "../Setup/StreamQuality";
import WorkerCount from "../General/WorkerCount";
import {Accordion, AccordionDetails, AccordionSummary, Box, makeStyles, Typography} from "@material-ui/core";
import Status from "../Setup/Status";
import Update from "../Update/Update";
import VideoThumbnails from "./VideoThumbnails";

const styles = makeStyles((theme) => ({
    accordionRoot: {
        border: '1px solid rgba(0, 0, 0, .125)',
        boxShadow: 'none',
        '&:not(:last-child)': {
            borderBottom: 0,
        },
        '&:before': {
            display: 'none',
        },
        '&$expanded': {
            margin: '0',
        },
    },
    expanded: {
        margin: "0"
    }
}))

export default function General() {
    const classes = styles();
    const [workerCount, setWorkerCount] = useState({availableThreads: 0, currentSetThreads: 0});
    const [videoThumbnailsEnabled, setVideoThumbnailsEnabled] = useState(false);
    const [streamQuality, setStreamQuality] = useState({resolution: 0, fps: 0});
    
    useEffect(() => {
        getCurrentSettings();
    }, [])
    
    async function getCurrentSettings() {
        const request = await fetch('setup/globalSettings', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (request.ok) {
            var response = await request.json();
            
            for (var x = 0; x < response.length; x++) {
                switch (response[x].key) {
                    case "workerCount":
                        var jsonConverted = JSON.parse(response[x].value);
                        setWorkerCount({availableThreads: jsonConverted.AvailableThreads, currentSetThreads: jsonConverted.CurrentSetThreads});
                        break;
                    case "generateVideoThumbnails":
                        setVideoThumbnailsEnabled((response[x].value === "True"));
                        break;
                    case "streamQuality":
                        console.log(JSON.parse(response[x].value));
                        setStreamQuality(JSON.parse(response[x].value));
                }
            }
        }
    }
    
    return (
        <div>
            <Update/>
            <Status/>
            <StreamQuality streamQuality={streamQuality} />
            <br/>
            <Accordion className={classes.accordionRoot} classes={{expanded: classes.expanded}}>
                <AccordionSummary>
                    <Typography>Show Advanced Settings</Typography>
                </AccordionSummary>
                <AccordionDetails>
                    <div>
                        <WorkerCount workerCount={workerCount} />
                        <VideoThumbnails generateVideoThumbnails={videoThumbnailsEnabled} />
                        <h2>Background Job Page</h2>
                        <a href="/hangfire">We use Hangfire to queue background jobs.</a>
                        <p>This controls 99% of the functions of voddy. Unless you know what you are doing, you should
                            not adjust any settings in that area. Doing so may break your current running instance. If
                            you have adjusted any jobs, and wish to revert the changes, restart the application.</p>
                    </div>
                </AccordionDetails>
            </Accordion>
        </div>
    )
}
