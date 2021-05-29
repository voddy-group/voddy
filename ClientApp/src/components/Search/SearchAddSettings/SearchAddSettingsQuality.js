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

export default function SearchAddSettingsQuality(quality) {
    const [qualityValue, setQualityValue] = useState({"resolution": 0, "fps": 0});
    const classes = styles();

    async function handleInputChange(e) {
        quality.setQuality(JSON.parse(e.target.value));
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
        </div>
    )
}