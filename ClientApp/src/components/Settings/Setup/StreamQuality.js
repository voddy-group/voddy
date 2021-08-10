import React, {useState, useEffect} from "react";
import {InputLabel, MenuItem, Select, Typography} from "@material-ui/core";

export default function StreamQuality(props) {
    const [qualityValue, setQualityValue] = useState({Resolution: 0, Fps: 0});


    useEffect(() => {
        console.log(JSON.stringify(props.streamQuality));
        if (props.streamQuality.Resolution !== undefined && props.streamQuality.Fps !== undefined) {
            setQualityValue(props.streamQuality);
        }
    }, [props.streamQuality])

    async function handleInputChange(e) {
        var parsedInput = JSON.parse(e.target.value);

        const response = await fetch('setup/globalSettings', {
            method: 'put',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "streamQuality": {
                    "resolution": parsedInput.Resolution,
                    "fps": parsedInput.Fps
                }
            })
        });

        if (response.ok) {
            setQualityValue({Resolution: parsedInput.Resolution, Fps: parsedInput.Fps})
        }
    }

    return (
        <div>
            <Typography variant={"h3"} color={"primary"}>Stream Quality</Typography>
            <p>Set the default stream quality for all streams. Streamer level orderrides this settings.</p>
            <InputLabel>Quality Value</InputLabel>
            <Select
                value={JSON.stringify(qualityValue)}
                onChange={handleInputChange}
                >
                <MenuItem value='{"Resolution":0,"Fps":0}'>Highest Quality</MenuItem>
                <MenuItem value='{"Resolution":1080,"Fps":60}'>1080p 60fps</MenuItem>
                <MenuItem value='{"Resolution":720,"Fps":60}'>720p 60fps</MenuItem>
                <MenuItem value='{"Resolution":720,"Fps":30}'>720p 30fps</MenuItem>
                <MenuItem value='{"Resolution":480,"Fps":30}'>480p 30fps</MenuItem>
                <MenuItem value='{"Resolution":360,"Fps":30}'>360p 30fps</MenuItem>
                <MenuItem value='{"Resolution":160,"Fps":30}'>160p 30fps</MenuItem>
            </Select>
            <p>Default is highest quality.</p>
            <p>If the quality is not availble the next best, lower quality will be chosen, or the same resolution but differrent fps will be chosen. More details can be found here.</p>
        </div>
    )
}