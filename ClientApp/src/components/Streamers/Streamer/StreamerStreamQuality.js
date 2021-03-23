import React, {useState, useEffect} from "react";
import loading from "../../../assets/images/loading.gif";
import cloneDeep from 'lodash/cloneDeep'

export default function StreamerStreamQuality(streamer) {
    const [isLoading, setIsLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [saveText, setSaveText] = useState("Save");
    const [currentQualityValue, setCurrentQualityValue] = useState("Fetching...");
    const [qualityValue, setQualityValue] = useState({resolution: 0, fps: 0});


    useEffect(() => {
        handleCurrentQualityOptions();
    }, [])

    async function handleCurrentQualityOptions() {
        if (streamer.quality !== void(0)) {
            setCurrentQualityValue(streamer.quality.Resolution + "p " + streamer.quality.Fps + " fps");
        } else {
            setQualityValue({resolution: 0, fps: 0})
            setCurrentQualityValue("Highest Quality.")
        }
    }

    function handleInputChange(e) {
        var parsedInput = JSON.parse(e.target.value);
        setQualityValue({resolution: parsedInput.resolution, fps: parsedInput.fps})
        setSaveText("Save");
    }

    async function handleSaveClick() {
        setIsLoading(true);
        setSaving(true);
        setSaveText("Saving..")
        
        var newStreamer = cloneDeep(streamer);
        if (qualityValue.resolution !== 0 && qualityValue.fps !== 0) {
            newStreamer.streamer.quality = JSON.stringify(qualityValue);
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

        if (response.ok) {
            setIsLoading(false);
            setSaving(false);
            setSaveText("Saved!");
            if (qualityValue.resolution === 0 && qualityValue.fps === 0) {
                setCurrentQualityValue("None.")
            } else {
                setCurrentQualityValue(qualityValue.resolution + "p " + qualityValue.fps + " fps");
            }
        }
    }
    
return (
    <div>
        <p>Stream quality:</p>
        <p>Current default quality: {currentQualityValue}</p>
        <select name="qualityOptions" onChange={handleInputChange}>
            <option value='{"resolution": 0, "fps": 0}'>Highest Quality</option>
            <option value='{"resolution": 1080, "fps": 60}'>1080p 60fps</option>
            <option value='{"resolution": 720, "fps": 60}'>720p 60fps</option>
            <option value='{"resolution": 720, "fps": 30}'>720p 30fps</option>
            <option value='{"resolution": 480, "fps": 30}'>480p 30fps</option>
            <option value='{"resolution": 360, "fps": 30}'>360p 30fps</option>
            <option value='{"resolution": 160, "fps": 30}'>160p 30fps</option>
        </select>
        <button onClick={handleSaveClick} disabled={saving}><img
            className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{saveText}
        </button>
    </div>
)
}