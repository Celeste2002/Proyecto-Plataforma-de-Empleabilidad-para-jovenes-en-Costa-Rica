import { Send, X } from 'lucide-react';
import { useState } from 'react';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { enviarMensaje } from '../api/employerApi.js';

export function EnviarMensajeModal({ candidateName, postulacionId, token, onClose }) {
  const [content, setContent] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  async function handleSubmit(e) {
    e.preventDefault();
    const trimmed = content.trim();

    if (!trimmed) {
      setErrorMessage('El mensaje no puede estar vacío.');
      return;
    }
    if (trimmed.length > 1000) {
      setErrorMessage('El mensaje no puede superar los 1000 caracteres.');
      return;
    }

    setIsSending(true);
    setErrorMessage('');

    try {
      await enviarMensaje(token, postulacionId, trimmed);
      setSuccessMessage('Mensaje enviado correctamente.');
      setContent('');
      setTimeout(onClose, 1500);
    } catch (error) {
      setErrorMessage(error.message);
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
        className="modal-card"
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
      >
        <div className="modal-card__header">
          <h3 id="modal-title" className="modal-card__title">
            Enviar mensaje a {candidateName}
          </h3>
          <button
            className="modal-card__close"
            disabled={isSending}
            onClick={onClose}
            type="button"
            aria-label="Cerrar"
          >
            <X size={20} />
          </button>
        </div>

        <form className="modal-card__form" onSubmit={handleSubmit} noValidate>
          <StatusMessage message={errorMessage} tone="error" />
          <StatusMessage message={successMessage} tone="success" />

          <label>
            Mensaje
            <textarea
              className="modal-card__textarea"
              disabled={isSending}
              maxLength={1000}
              name="content"
              onChange={(e) => setContent(e.target.value)}
              placeholder="Escribe tu mensaje al candidato..."
              required
              rows={5}
              value={content}
            />
            <span className="modal-card__char-count">
              {content.length}/1000 caracteres
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
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
