import React, { useState } from "react";
import "../../assets/styles/StreamSearch.css";
import loading from "../../assets/images/loading.gif";


export default function RenderSearchRow(searchData) {
    const [isLoading, setIsLoading] = useState(false);
    const [addButtontext, setAddButtonText] = useState("Add");
    const [addButtonClass, setAddButtonClass] = useState("add")
    const [addButtonDisabled, setAddButtonDisabled] = useState(false)
    searchData = searchData.data;
    
    function handleButtonClicked() {
        setIsLoading(true);
        setAddButtonText(null);
        
        changeStreamStatus();
    }
    
    async function changeStreamStatus() {
        var body = {
            "streamId": searchData.id,
            "displayName": searchData.display_name,
            "username": searchData.broadcaster_login,
            "isLive": Boolean(searchData.is_live),
            "thumbnailUrl": searchData.thumbnail_url
        }
        
        console.log(JSON.stringify(body));
        
        const response = await fetch('database/streamer' +
            '?value=true',
            {
            method: 'patch',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body)
        });
        
        if (response.ok) {
            setAddButtonText("Added!");
            setAddButtonClass("greyed")
            setIsLoading(false);
            setAddButtonDisabled(true);
        }
    }
    
    return (
        <tr key={searchData.id}>
            <td><img className={'thumbnail'} src={searchData.thumbnail_url}/></td>
            <td>{searchData.display_name}</td>
            <td>{searchData.title}</td>
            <td><button disabled={addButtonDisabled} className={addButtonClass} onClick={handleButtonClicked}><img className={isLoading ? 'loading' : 'hidden'} src={loading} />{addButtontext}</button></td>
        </tr>
    )
}
