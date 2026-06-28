import { MessageSquare, Send, X } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { enviarMensaje, getMensajesPostulacion } from '../api/employerApi.js';

function formatDateTime(dateString) {
  return new Date(dateString).toLocaleString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function CandidatoChatModal({
  candidateName,
  candidateEmail,
  postulacionId,
  token,
  onClose,
  onMessageSent,
}) {
  const [messages, setMessages] = useState([]);
  const [content, setContent] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const messagesEndRef = useRef(null);

  useEffect(() => {
    let isMounted = true;
    setIsLoading(true);
    setErrorMessage('');

    getMensajesPostulacion(token, postulacionId)
      .then((data) => {
        if (isMounted) setMessages(data);
      })
      .catch((error) => {
        if (isMounted) setErrorMessage(error.message);
      })
      .finally(() => {
        if (isMounted) setIsLoading(false);
      });

    return () => {
      isMounted = false;
    };
  }, [postulacionId, token]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ block: 'end' });
  }, [messages]);

  async function handleSubmit(event) {
    event.preventDefault();
    const trimmed = content.trim();

    if (!trimmed) {
      setErrorMessage('El mensaje no puede estar vacio.');
      return;
    }

    if (trimmed.length > 2000) {
      setErrorMessage('El mensaje no puede superar los 2000 caracteres.');
      return;
    }

    setIsSending(true);
    setErrorMessage('');

    try {
      const sentMessage = await enviarMensaje(token, postulacionId, trimmed);
      setMessages((current) => [...current, sentMessage]);
      setContent('');
      onMessageSent?.(sentMessage);
    } catch (error) {
      setErrorMessage(error.validationErrors?.[0] ?? error.message);
    } finally {
      setIsSending(false);
    }
  }

  return (
    <div
      className="modal-backdrop"
      onClick={() => !isSending && onClose()}
      role="presentation"
    >
      <div
        className="modal-card chat-modal"
        onClick={(event) => event.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="chat-modal-title"
      >
        <div className="modal-card__header">
          <div>
            <h3 id="chat-modal-title" className="modal-card__title">
              Chat con {candidateName}
            </h3>
            <p className="chat-modal__subtitle">{candidateEmail}</p>
          </div>
          <button
            className="modal-card__close"
            disabled={isSending}
            onClick={onClose}
            type="button"
            aria-label="Cerrar chat"
          >
            <X size={20} />
          </button>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        <div className="chat-modal__messages" aria-live="polite">
          {isLoading && <p className="empty-state">Cargando mensajes...</p>}

          {!isLoading && messages.length === 0 && (
            <div className="chat-modal__empty">
              <MessageSquare aria-hidden="true" size={24} />
              <p>No hay mensajes enviados todavia.</p>
            </div>
          )}

          {!isLoading && messages.map((message) => (
            <article className="chat-bubble chat-bubble--employer" key={message.id}>
              <p>{message.body}</p>
              <time dateTime={message.sentAtUtc}>{formatDateTime(message.sentAtUtc)}</time>
            </article>
          ))}
          <div ref={messagesEndRef} />
        </div>

        <form className="modal-card__form" onSubmit={handleSubmit} noValidate>
          <label>
            Mensaje
            <textarea
              className="modal-card__textarea"
              disabled={isSending}
              maxLength={2000}
              name="content"
              onChange={(event) => setContent(event.target.value)}
              placeholder="Escribe un mensaje para el candidato..."
              required
              rows={4}
              value={content}
            />
            <span className="modal-card__char-count">
              {content.length}/2000 caracteres
            </span>
          </label>

          <div className="modal-card__actions">
            <button
              className="primary-action"
              disabled={isSending || !content.trim()}
              type="submit"
            >
              <Send aria-hidden="true" size={15} />
              {isSending ? 'Enviando...' : 'Enviar mensaje'}
            </button>
            <button
              className="secondary-action"
              disabled={isSending}
              onClick={onClose}
              type="button"
            >
              Cerrar
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
