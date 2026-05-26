export function FieldError({ errors }) {
  if (!errors.length) {
    return null;
  }

  return (
    <div className="field-error" role="alert">
      {errors.map((error) => (
        <p key={error}>{error}</p>
      ))}
    </div>
  );
}
