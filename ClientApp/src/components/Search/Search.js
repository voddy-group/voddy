import React, {useEffect, useState} from "react";
import RenderSearchRow from "./RenderRow";
import "../../assets/styles/StreamSearch.css";


export default function Search() {
    const [searchValue, setSearchValue] = useState("");
    const [searchData, setSearchData] = useState([]);
    const [hideSuggested, setHideSuggested] = useState(false);

    function handleChangeSearchValue(e) {
        setSearchValue(e.target.value);
    }

    useEffect(() => {
        getFollowedChannels();
    }, [])
    
    useEffect(() => {
        const delayDebounceFn = setTimeout(() => getSearch(), 1500)
        return () => clearTimeout(delayDebounceFn)
    }, [searchValue])

    async function getFollowedChannels() {
        var response = await fetch('twitchApi/followed',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        var returnedData = await response.json();
        
        setSearchData(returnedData.data);
    }
    
    async function getSearch() {
        if (searchValue !== void (0) && searchValue !== "") {
            const response = await fetch('twitchApi/stream/search' +
                '?term=' + searchValue, {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const returnedData = await response.json();

            setHideSuggested(true);

            var streams = await getStreamers();
            
            // TODO make this efficient
            for (var x = 0; x < returnedData.data.length; x++) {
                for (var i = 0; i < streams.data.length; i++) {
                    if (returnedData.data[x].id === streams.data[i].streamerId) {
                        returnedData.data[x].alreadyAdded = true;
                        break;
                    } else {
                        returnedData.data[x].alreadyAdded = false;
                    }
                }
            }
            
            setSearchData(returnedData.data);
        }
    }

    async function getStreamers() {
        const response = await fetch('database/streamers',
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        return await response.json();
    }

    return (
        <div>
            <h1>Search for streams</h1>
            <input onChange={handleChangeSearchValue}/>
            <p className={hideSuggested ? 'hidden' : ''}>Followed channels:</p>
            <table>
                <tbody>
                {searchData.map(searchedData => <RenderSearchRow key={searchedData.id} searchedData={searchedData} />)}
                </tbody>
            </table>
        </div>
    )
}