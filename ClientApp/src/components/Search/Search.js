import React, {useState, useEffect} from "react";
import RenderSearchRow from "./RenderRow";
import "../../assets/styles/StreamSearch.css";


export default function Search() {
    const [searchValue, setSearchValue] = useState("");
    const [searchData, setSearchData] = useState([]);

    function handleChangeSearchValue(e) {
        setSearchValue(e.target.value);
    }

    useEffect(() => {
        const delayDebounceFn = setTimeout(() => getSearch(), 1500)
        return () => clearTimeout(delayDebounceFn)
    }, [searchValue])

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
            
            var streams = await getStreamers();
            
            // TODO make this efficient
            for (var x = 0; x < returnedData.data.length; x++) {
                for (var i = 0; i < streams.data.length; i++) {
                    if (returnedData.data[x].id === streams.data[i].streamId) {
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
            <table>
                <tbody>
                {searchData.map(searchedData => <RenderSearchRow key={searchedData.id} searchedData={searchedData} />)}
                </tbody>
            </table>
        </div>
    )
}