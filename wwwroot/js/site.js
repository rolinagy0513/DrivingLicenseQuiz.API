$(document).ready(function() {
    let currentQuiz = null;
    let currentQuestionIndex = 0;
    let userAnswers = [];
    let quizTimer = null;
    let timeRemaining = 0;

    // Get button references
    const startQuizBtn = document.getElementById('startQuizBtn');
    const nextQuestionBtn = document.getElementById('nextQuestionBtn');
    const submitQuizBtn = document.getElementById('submitQuizBtn');
    const logoutLink = document.getElementById('logoutLink');
    const viewDetailsBtn = document.getElementById('viewDetailsBtn');

    // Check session status on page load
    checkSession();

    // Event handlers using vanilla JavaScript
    startQuizBtn.addEventListener('click', async function(e) {
        e.preventDefault();
        await startQuiz();
    });

    nextQuestionBtn.addEventListener('click', nextQuestion);
    submitQuizBtn.addEventListener('click', submitQuiz);
    logoutLink.addEventListener('click', logout);

    function checkSession() {
        fetch('/api/session/validate', {
            method: 'GET',
            credentials: 'include'
        })
        .then(response => response.json())
        .then(response => {
            // User is logged in
            document.getElementById('loginNavItem').classList.add('d-none');
            document.getElementById('userNavItem').classList.remove('d-none');
            document.getElementById('logoutNavItem').classList.remove('d-none');
            document.getElementById('userName').textContent = response.name;
            startQuizBtn.disabled = false;
            document.getElementById('loginPrompt').classList.add('d-none');
            loadQuizHistory();
        })
        .catch(() => {
            // User is not logged in
            document.getElementById('loginNavItem').classList.remove('d-none');
            document.getElementById('userNavItem').classList.add('d-none');
            document.getElementById('logoutNavItem').classList.add('d-none');
            startQuizBtn.disabled = true;
            document.getElementById('loginPrompt').classList.remove('d-none');
        });
    }

    async function startQuiz() {
        console.log('Starting quiz...');
        const errorDiv = document.getElementById('quizError');
        errorDiv.classList.add('d-none');
        errorDiv.textContent = '';

        try {
            // Check if we have a session cookie
            const sessionCookie = document.cookie.split('; ').find(row => row.startsWith('SessionId='));
            console.log('Session cookie present:', !!sessionCookie);
            if (!sessionCookie) {
                throw new Error('No session cookie found. Please log in again.');
            }
            
            // Show loading state
            startQuizBtn.disabled = true;
            startQuizBtn.textContent = 'Starting quiz...';
            console.log('Button state updated to loading');

            // Make the API call
            console.log('Making API request to /api/quiz/start...');
            const response = await fetch('/api/quiz/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                credentials: 'include' // Important for cookies
            });
            
            console.log('Response status:', response.status);
            console.log('Response headers:', Object.fromEntries(response.headers.entries()));

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server responded with error:', {
                    status: response.status,
                    statusText: response.statusText,
                    body: errorText
                });
                throw new Error(`Server error: ${response.status} ${response.statusText} - ${errorText}`);
            }

            const data = await response.json();
            console.log('Quiz data received successfully:', {
                quizId: data.quizId,
                questionCount: data.questions?.length,
                timeLimit: data.timeLimitMinutes
            });

            // Store quiz data
            currentQuiz = {
                id: data.quizId,
                questions: data.questions,
                timeLimit: data.timeLimitMinutes,
                startTime: new Date()
            };
            currentQuestionIndex = 0;
            userAnswers = [];
            timeRemaining = data.timeLimitMinutes * 60;
            console.log('Quiz data stored in memory');

            // Update UI
            document.getElementById('welcomeSection').classList.add('d-none');
            document.getElementById('quizHistorySection').classList.add('d-none');
            document.getElementById('quizSection').classList.remove('d-none');
            nextQuestionBtn.classList.remove('d-none');
            submitQuizBtn.classList.add('d-none');
            console.log('UI updated to show quiz content');

            // Start the quiz
            startTimer();
            displayQuestion();
            console.log('Quiz timer started and first question displayed');

        } catch (error) {
            console.error('Error starting quiz:', {
                name: error.name,
                message: error.message,
                stack: error.stack
            });
            
            // Show error to user
            errorDiv.textContent = error.message || 'Failed to start quiz. Please try again.';
            errorDiv.classList.remove('d-none');
            
            // Reset button state
            startQuizBtn.disabled = false;
            startQuizBtn.textContent = 'Start Quiz';
        }
    }

    function displayQuestion() {
        const question = currentQuiz.questions[currentQuestionIndex];
        $('#questionText').text(question.text);
        $('#currentQuestion').text(currentQuestionIndex + 1);

        // Handle question image
        const $questionImage = $('#questionImage');
        if (question.imageUrl) {
            $questionImage.attr('src', question.imageUrl).removeClass('d-none');
        } else {
            $questionImage.addClass('d-none');
        }

        // Display answers
        const $answersContainer = $('#answersContainer');
        $answersContainer.empty();

        question.answers.forEach(answer => {
            const $answer = $('<div>')
                .addClass('answer-option')
                .text(answer.text)
                .data('answerId', answer.id)
                .on('click', function() {
                    $('.answer-option').removeClass('selected');
                    $(this).addClass('selected');
                });
            $answersContainer.append($answer);
        });

        // Show/hide submit button
        if (currentQuestionIndex === currentQuiz.questions.length - 1) {
            $('#nextQuestionBtn').addClass('d-none');
            $('#submitQuizBtn').removeClass('d-none');
        }
    }

    function nextQuestion() {
        const selectedAnswer = $('.answer-option.selected');
        if (selectedAnswer.length === 0) {
            alert('Please select an answer');
            return;
        }

        const answerId = selectedAnswer.data('answerId');
        const questionId = currentQuiz.questions[currentQuestionIndex].id;

        userAnswers.push({
            questionId: questionId,
            answerId: answerId
        });

        currentQuestionIndex++;
        displayQuestion();
    }

    function submitQuiz() {
        const selectedAnswer = $('.answer-option.selected');
        if (selectedAnswer.length === 0) {
            alert('Please select an answer');
            return;
        }

        const answerId = selectedAnswer.data('answerId');
        const questionId = currentQuiz.questions[currentQuestionIndex].id;

        userAnswers.push({
            questionId: questionId,
            answerId: answerId
        });

        stopTimer();

        fetch(`/api/quiz/${currentQuiz.id}/submit`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userAnswers)
        })
        .then(response => response.json())
        .then(data => {
            $('#finalScore').text(data.score);
            $('#timeTaken').text(formatTime(currentQuiz.timeLimitMinutes * 60 - timeRemaining));
            
            // Update the modal content to include the history message
            const modalBody = document.querySelector('#quizResultsModal .modal-body');
            const historyMessage = document.createElement('div');
            historyMessage.className = 'alert alert-info mt-3';
            historyMessage.innerHTML = '<i class="fas fa-info-circle"></i> For detailed review of your answers, please check the quiz history below.';
            modalBody.appendChild(historyMessage);
            
            // Hide the view details button
            const viewDetailsBtn = document.getElementById('viewDetailsBtn');
            if (viewDetailsBtn) {
                viewDetailsBtn.style.display = 'none';
            }
            
            $('#quizResultsModal').modal('show');
            loadQuizHistory();
        })
        .catch(error => {
            console.error('Error submitting quiz:', error);
            alert('Failed to submit quiz');
        });
    }

    function startTimer() {
        stopTimer();
        updateTimerDisplay();
        quizTimer = setInterval(function() {
            timeRemaining--;
            updateTimerDisplay();

            if (timeRemaining <= 0) {
                stopTimer();
                submitQuiz();
            }
        }, 1000);
    }

    function stopTimer() {
        if (quizTimer) {
            clearInterval(quizTimer);
            quizTimer = null;
        }
    }

    function updateTimerDisplay() {
        const minutes = Math.floor(timeRemaining / 60);
        const seconds = timeRemaining % 60;
        $('#timer').text(`Time: ${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`);
    }

    function formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
    }

    function loadQuizHistory() {
        fetch('/api/quiz/history', {
            method: 'GET',
            credentials: 'include'
        })
        .then(response => response.json())
        .then(data => {
            const $historyTable = $('#quizHistoryTable');
            $historyTable.empty();

            data.forEach(quiz => {
                const date = new Date(quiz.completedAt).toLocaleString();
                const row = `
                    <tr class="quiz-history-row">
                        <td>${date}</td>
                        <td>${quiz.score}/${quiz.totalQuestions}</td>
                        <td>${quiz.totalQuestions}</td>
                        <td>
                            <button type="button" class="btn btn-sm btn-primary view-details" onclick="viewQuizDetails(${quiz.id})">
                                View Details
                            </button>
                        </td>
                    </tr>
                `;
                $historyTable.append(row);
            });

            $('#quizHistorySection').removeClass('d-none');
        })
        .catch(error => {
            console.error('Failed to load quiz history:', error);
        });
    }

    function viewQuizDetails(quizId) {
        if (!quizId || isNaN(quizId)) {
            console.error('Invalid quiz ID:', quizId);
            alert('Invalid quiz ID. Please try again.');
            return;
        }

        console.log('Fetching quiz details for quiz ID:', quizId);

        fetch(`/api/quiz/${quizId}`, {
            method: 'GET',
            credentials: 'include'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Quiz details received:', data);
            
            // Create and show modal with quiz details
            const modal = `
                <div class="modal fade" id="quizDetailsModal" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">Quiz Review</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <div class="alert ${data.questions.length === 0 ? 'alert-success' : 'alert-info'} mb-4">
                                    <h5 class="alert-heading">${data.message}</h5>
                                    <div class="mt-2">
                                        <strong>Your Score:</strong> ${data.score}/${data.totalQuestions}
                                    </div>
                                    ${data.questions.length > 0 ? `
                                        <div class="mt-2">
                                            <strong>Wrong Answers:</strong> ${data.wrongAnswersCount}
                                        </div>
                                    ` : ''}
                                </div>
                                ${data.questions.length > 0 ? `
                                    <div class="wrong-answers-section">
                                        <h5 class="mb-3">Questions to Review:</h5>
                                        ${data.questions.map((q, index) => `
                                            <div class="question-review mb-4 p-3 border rounded">
                                                <h6 class="mb-2">Question ${index + 1}</h6>
                                                <p class="mb-2">${q.text}</p>
                                                ${q.imageUrl ? `<img src="${q.imageUrl}" class="img-fluid mb-2" alt="Question Image">` : ''}
                                                <div class="answer-review">
                                                    <div class="your-answer mb-2">
                                                        <strong class="text-danger">Your Answer:</strong> ${q.userAnswer.text}
                                                    </div>
                                                    <div class="correct-answer">
                                                        <strong class="text-success">Correct Answer:</strong> ${q.correctAnswer.text}
                                                    </div>
                                                </div>
                                            </div>
                                        `).join('')}
                                    </div>
                                ` : ''}
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            // Remove existing modal if any
            $('#quizDetailsModal').remove();
            
            // Add new modal to body and show it
            $('body').append(modal);
            const modalInstance = new bootstrap.Modal('#quizDetailsModal');
            modalInstance.show();
        })
        .catch(error => {
            console.error('Failed to load quiz details:', error);
            alert('Failed to load quiz details. Please try again.');
        });
    }

    function logout() {
        fetch('/api/session/logout', {
            method: 'POST',
            credentials: 'include'
        })
        .then(() => {
            window.location.reload();
        })
        .catch(error => {
            console.error('Failed to logout:', error);
            alert('Failed to logout');
        });
    }

    // Handle quiz results modal
    $('#quizResultsModal').on('hidden.bs.modal', function() {
        $('#quizSection').addClass('d-none');
        $('#welcomeSection').removeClass('d-none');
    });
}); 