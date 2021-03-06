import React, {useState} from "react";
import "../../assets/styles/StreamSearch.css";
import loading from "../../assets/images/loading.gif";


export default function RenderSearchRow(searchedData) {
    const [isLoading, setIsLoading] = useState(false);
    const [addButtontext, setAddButtonText] = useState("Add");
    const [addButtonClass, setAddButtonClass] = useState("add");
    const [addButtonDisabled, setAddButtonDisabled] = useState(false);
    const [alreadyAdded, setAlreadyAdded] = useState(false);
    const [newSearchData] = useState(searchedData.searchedData);
    

    if (newSearchData.alreadyAdded && !alreadyAdded) {
        added();
        setAlreadyAdded(true);
    }

    function handleButtonClicked() {
        setIsLoading(true);
        setAddButtonText(null);

        changeStreamStatus();
    }

    async function changeStreamStatus() {
        var body = {
            "streamerId": newSearchData.id,
            "displayName": newSearchData.display_name,
            "username": newSearchData.broadcaster_login,
            "isLive": Boolean(newSearchData.is_live),
            "thumbnailUrl": newSearchData.thumbnail_url
        }

        console.log(JSON.stringify(body));

        const response = await fetch('database/streamer' +
            '?value=true',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });

        if (response.ok) {
            added();
        }
    }

    function added() {
        setAddButtonText("Added!");
        setAddButtonClass("greyed")
        setIsLoading(false);
        setAddButtonDisabled(true);
    }

    return (
        <tr>
            <td><img className={'thumbnail'} alt="thumbnail" src={newSearchData.thumbnail_url}/></td>
            <td>{newSearchData.display_name}</td>
            <td>{newSearchData.title}</td>
            <td>
                <button disabled={addButtonDisabled} className={addButtonClass} onClick={handleButtonClicked}><img
                    className={isLoading ? 'loading' : 'hidden'} alt="loading" src={loading}/>{addButtontext}</button>
            </td>
        </tr>
    )
}
