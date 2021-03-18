import React, {useState, useEffect} from "react";
import loading from "../../assets/images/loading.gif";

export default function WorkerCount() {
    const [threadCount, setThreadCount] = useState({availableThreads: 0, currentSetThreads: 0});
    const [isLoading, setIsLoading] = useState(false);
    const [saving, setSaving] = useState(false);
    const [saveText, setSaveText] = useState("Save");
    const [threadValue, setThreadValue] = useState(-1);

    
    useEffect(() => {
        getThreadCount();
    }, [])

    async function getThreadCount() {
        const response = await fetch('setup/threads', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        var returnedData = await response.json();
        
        if (returnedData.currentSetThreads === 0) {
            setThreadCount({availableThreads: returnedData.availableThreads, currentSetThreads: returnedData.availableThreads})
        } else {
            setThreadCount({availableThreads: returnedData.availableThreads, currentSetThreads: returnedData.currentSetThreads})
        }
    }

    function handleInputChange(e) {
        console.log(e.target.value)
        if (e.target.value !== null && e.target.value !== "") {
            setThreadValue(e.target.value);
        } else {
            setThreadValue(-1);
        }
        setSaveText("Save");
    }

    async function handleSaveClick() {
        setIsLoading(true);
        setSaving(true);
        setSaveText("Saving..")

        
        const response = await fetch('setup/threads' +
            '?threadCount=' + threadValue, {
            method: 'put',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            setIsLoading(false);
            setSaving(false);
            setSaveText("Saved!");
            if (threadValue !== -1) {
                setThreadCount({availableThreads: threadCount.availableThreads, currentSetThreads: threadValue});
            } else {
                setThreadCount({availableThreads: threadCount.availableThreads, currentSetThreads: threadCount.availableThreads});
            }
        }

    }

    return (
        <div>
            <p>Advanced settings: Thread Count</p>
            <p>You can set the thread count to a lower number to limit the amount of background processes the
                application does.</p>
            <p>If you have to limit the speed of your stream downloads, you should set this to 1. Do note that this
                slows down downloading/fetching streams and many other background processes. DO NOT change this value if
                you do not need to.</p>
            <p>If you want to limit your download speed, I would advise using some kind of third-party thing that limits
                the download speed of this application at a lower level.</p>
            <p>Setting this number to a value higher than your computers thread limit will result in no changes.</p>
            <p>Current computer thread count: {threadCount.availableThreads}</p>
            <p>Current computer thread count: {threadCount.currentSetThreads}</p>
            <input type="number" max={threadCount} min="0" onChange={handleInputChange}
                   placeholder="Enter a number here."/>
            <button onClick={handleSaveClick} disabled={saving}><img
                className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{saveText}
            </button>
            <p>You will need to restart the application after saving these changes.</p>
            <p>Leaving the input empty and saving will reset the thread count to default (all threads).</p>
        </div>
    )
}