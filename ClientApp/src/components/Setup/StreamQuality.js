import React, {useState, useEffect} from "react";
import loading from "../../assets/images/loading.gif";

export default function StreamQuality() {
    const [isLoading, setIsLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [saveText, setSaveText] = useState("Save");
    const [currentQualityValue, setCurrentQualityValue] = useState("Fetching...");
    const [qualityValue, setQualityValue] = useState({resolution: 0, fps: 0});


    useEffect(() => {
        getCurrentQualityOptions();
    }, [])

    async function getCurrentQualityOptions() {
        const request = await fetch('setup/quality', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (request.status === 200) {
            var response = await request.json();
            setCurrentQualityValue(response.Resolution + "p " + response.Fps + " fps");
        } else if (request.status === 204) {
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


        const response = await fetch('setup/quality' +
            '?resolution=' + qualityValue.resolution +
            '&fps=' + qualityValue.fps, {
            method: 'put',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            setIsLoading(false);
            setSaving(false);
            setSaveText("Saved!");
            if (qualityValue.resolution === 0 && qualityValue.fps === 0) {
                setCurrentQualityValue("Highest Quality.")
            } else {
                setCurrentQualityValue(qualityValue.resolution + "p " + qualityValue.fps + " fps");
            }
        }
    }

    return (
        <div>
            <p>Default stream quality.</p>
            <p>Set the default stream quality for all streams. Streamer level orderrides this settings.</p>
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
            <p>Default is highest quality.</p>
            <p>If the quality is not availble the next best, lower quality will be chosen, or the same resolution but differrent fps will be chosen. More details can be found here.</p>
            <button onClick={handleSaveClick} disabled={saving}><img
                className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{saveText}
            </button>
        </div>
    )
}