import profilePic from "./assets/vamp.jpg"

function Card() {
    return (
        <div className="card">
            <img className="card-image" src={profilePic} alt="profile picture"></img>
            <h2 className="user-name">aaaa</h2>
            <p className="posts">idk</p>
        </div>
    );
}

export default Card