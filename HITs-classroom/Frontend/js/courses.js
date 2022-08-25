window.addEventListener('load', function () {

    const searchBtn = document.querySelector('#searchCourseSubmitButton');
    if (searchBtn) {
        searchBtn.addEventListener("click", findCourses);
    }

    const searchByIdBtn = document.querySelector('#searchCourseByIdSubmitButton');
    if (searchByIdBtn) {
        searchByIdBtn.addEventListener("click", findCourseById);
    }

    findActiveCourses();
});

function findActiveCourses() {
    console.log('findActiveCourses');
    var data = new Object();
    data.courseState = "ACTIVE";
    data = JSON.stringify(data);
    post(
        'https://localhost:7284/api/Courses/Courses',
        data
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => console.error(error))
}

function findCourses() {
    console.log('findCourses');
    var data = new Object();
    for (const pair of new FormData(document.querySelector('#searchCoursesForm'))) {
        if (pair[1] != null && pair[1] != '') {
            data[pair[0]] = pair[1];
        }
    }
    data = JSON.stringify(data);
    post(
        'https://localhost:7284/api/Courses/Courses',
        data
    )
        .then(response => addCoursesToPage(convertCoursesFromJsonToArray(response)))
        .catch(error => console.error(error))
}

function findCourseById() {
    console.log('findCourseById');
    var Id = document.querySelector('#courseId_Serach').value;
    get(
        'https://localhost:7284/api/Courses/Get/' + Id.toString()
    )
        .then(response => addCoursesToPage([prepareCourseFromJson(response)]))
        .catch(error => console.error(error));
}

function post(url, data) {
    return fetch(url,
        {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: data
        }
    ).then(response => response.json());
}

function get(url) {
    return fetch(url,
        {
            method: "GET",
        }
    ).then(response => response.json());
}

function convertCoursesFromJsonToArray(json) {
    console.log('convertCoursesFromJsonToArray');
    let newArray = [];
    for (let i = 0; i < json.length; i++) {
        newArray.push(prepareCourseFromJson(json[i]));
    }

    return newArray;
}

function prepareCourseFromJson(course) {
    console.log('prepareCourseFromJson');
    let courseClone = document.querySelector('.course-card').cloneNode(true);
    courseClone.classList.remove('d-none');

    let courseName = courseClone.querySelector('.course-name');
    courseName.innerText = course['name'];
    let courseSection = courseClone.querySelector('.course-section');
    courseSection.innerText = course['section'];
    let courseDescriptionHeading = courseClone.querySelector('.course-description-heading');
    courseDescriptionHeading.innerText = course['descriptionHeading'];
    let courseEnrollmentCode = courseClone.querySelector('.course-enrollment-code');
    courseEnrollmentCode.innerText = course['enrollmentCode'];
    let courseState = courseClone.querySelector('.course-state');
    courseState.innerText = course['courseState'];

    return courseClone;
}

function addCoursesToPage(courses) {
    console.log('addCoursesToPage');
    let placeForCourses = document.querySelector('#allCourses');
    placeForCourses.innerHTML = '';
    for (let i = 0; i < courses.length; i++) {
        placeForCourses.appendChild(courses[i]);
    }
}