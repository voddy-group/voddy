import React, {useState, useEffect} from "react";
import cloneDeep from 'lodash/cloneDeep'
import {InputLabel, makeStyles, MenuItem, Select, Snackbar, Typography} from "@material-ui/core";
import {Alert} from "@material-ui/lab";

const styles = makeStyles((theme) => ({
    root: {
        display: "flex"
    },
    grow: {
        flexGrow: 1,
        minWidth: 50
    },
    liveText: {
        alignSelf: "center"
    },
    select: {
        width: 150
    }
}));

export default function StreamerStreamQuality(streamer) {
    const [qualityValue, setQualityValue] = useState({"resolution": 0, "fps": 0});
    const [snackbarOpen, setSnackbarOpen] = useState(false);
    const classes = styles();

    useEffect(() => {
        if (streamer.streamer.quality !== void (0) && streamer.streamer.quality !== null) {
            setQualityValue(JSON.parse(streamer.streamer.quality));
        }
    }, [])

    async function handleInputChange(e) {
        var parsedInput = JSON.parse(e.target.value);

        var newStreamer = cloneDeep(streamer);
        if (parsedInput.resolution !== 0 && parsedInput.fps !== 0) {
            newStreamer.streamer.quality = JSON.stringify(parsedInput);
        } else {
            newStreamer.streamer.quality = null;
        }

        const response = await fetch('database/streamer',
            {
                method: 'put',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newStreamer.streamer)
            });

        if (!response.ok) {
            setSnackbarOpen(true);
        } else {
            setQualityValue(parsedInput);
        }
    }

    function handleSnackbarClose(event, reason) {
        if (reason === "clickaway") {
            return;
        }

        setSnackbarOpen(false);
    }

    return (
        <div className={classes.root}>
            <Typography className={classes.liveText} variant={"body1"}>Stream Quality</Typography>
            <div className={classes.grow}/>
            <Select className={classes.select} value={JSON.stringify(qualityValue)} name="qualityOptions"
                    onChange={handleInputChange}>
                <MenuItem value='{"resolution":0,"fps":0}'>Highest Quality</MenuItem>
                <MenuItem value='{"resolution":1080,"fps":60}'>1080p 60fps</MenuItem>
                <MenuItem value='{"resolution":720,"fps":60}'>720p 60fps</MenuItem>
                <MenuItem value='{"resolution":720,"fps":30}'>720p 30fps</MenuItem>
                <MenuItem value='{"resolution":480,"fps":30}'>480p 30fps</MenuItem>
                <MenuItem value='{"resolution":360,"fps":30}'>360p 30fps</MenuItem>
                <MenuItem value='{"resolution":160,"fps":30}'>160p 30fps</MenuItem>
            </Select>
            <Snackbar open={snackbarOpen} autoHideDuration={6000} onClose={handleSnackbarClose}>
                <Alert onClose={handleSnackbarClose} severity="error">
                    Could not save settings! Please check your network configuration.
                </Alert>
            </Snackbar>
        </div>
    )
}