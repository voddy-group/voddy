import React from "react";

export default function RenderSearchRow(props) {
    return (
        <tr key={props.id}>
            <td><img src={props.thumbnail_url}/></td>
            <td>{props.display_name}</td>
            <td>{props.title}</td>
        </tr>
    )
}
